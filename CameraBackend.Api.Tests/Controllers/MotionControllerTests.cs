using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Xunit;
using CameraBackend.Api.Controllers;
using CameraBackend.Api.Models;

namespace CameraBackend.Api.Tests.Controllers;

public class MotionControllerTests
{
    private readonly MotionController _controller;
    private readonly TestServer _testServer;
    private readonly WebSocketClient _client;

    public MotionControllerTests()
    {
        _controller = new MotionController();
        _testServer = new TestServer(new WebHostBuilder()
            .UseStartup<TestStartup>());
        _client = _testServer.CreateWebSocketClient();
    }

    [Fact]
    public async Task HandleMotion_ReturnsStatus400_WhenNotWebSocketRequest()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext()
        {
            HttpContext = httpContext
        };

        // Act
        await _controller.HandleMotion();

        // Assert
        Assert.Equal(StatusCodes.Status400BadRequest, httpContext.Response.StatusCode);
    }

    [Theory]
    [InlineData(true)]  // Motion detected
    [InlineData(false)] // No motion
    public async Task BroadcastMotionDetection_SendsAccuratePirData(bool motionDetected)
    {
        // Arrange
        var socket = await _client.ConnectAsync(new Uri("ws://localhost/motion"), CancellationToken.None);
        var expectedData = new PirSensorData
        {
            Timestamp = DateTime.UtcNow,
            MotionDetected = motionDetected
        };

        try
        {
            // Act
            await MotionController.BroadcastPirData(expectedData);

            // Assert
            var buffer = new byte[1024];
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
            var receivedData = JsonSerializer.Deserialize<PirSensorData>(receivedMessage);
            
            Assert.NotNull(receivedData);
            Assert.Equal(expectedData.MotionDetected, receivedData.MotionDetected);
        }
        finally
        {
            if (socket.State == WebSocketState.Open)
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
        }
    }

    [Fact]
    public async Task HandleMotion_HandlesMultipleClients()
    {
        // Arrange
        var socket1 = await _client.ConnectAsync(new Uri("ws://localhost/motion"), CancellationToken.None);
        var socket2 = await _client.ConnectAsync(new Uri("ws://localhost/motion"), CancellationToken.None);

        try
        {
            // Simulate a sequence of PIR readings
            var pirReadings = new[]
            {
                new PirSensorData { Timestamp = DateTime.UtcNow, MotionDetected = true },
                new PirSensorData { Timestamp = DateTime.UtcNow.AddSeconds(1), MotionDetected = false },
                new PirSensorData { Timestamp = DateTime.UtcNow.AddSeconds(2), MotionDetected = true }
            };

            foreach (var reading in pirReadings)
            {
                // Act
                await MotionController.BroadcastPirData(reading);

                // Assert for both clients
                foreach (var socket in new[] { socket1, socket2 })
                {
                    var buffer = new byte[1024];
                    var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var receivedData = JsonSerializer.Deserialize<PirSensorData>(message);

                    Assert.NotNull(receivedData);
                    Assert.Equal(reading.MotionDetected, receivedData.MotionDetected);
                }
            }
        }
        finally
        {
            foreach (var socket in new[] { socket1, socket2 })
            {
                if (socket.State == WebSocketState.Open)
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
            }
        }
    }

    [Fact]
    public async Task HandleMotion_DetectsMotionStateChanges()
    {
        // Arrange
        var socket = await _client.ConnectAsync(new Uri("ws://localhost/motion"), CancellationToken.None);
        var previousState = false;

        try
        {
            // Simulate motion detection sequence: no motion -> motion detected -> no motion
            var pirSequence = new[]
            {
                new PirSensorData { MotionDetected = false, Timestamp = DateTime.UtcNow },
                new PirSensorData { MotionDetected = true, Timestamp = DateTime.UtcNow.AddSeconds(1) },
                new PirSensorData { MotionDetected = false, Timestamp = DateTime.UtcNow.AddSeconds(2) }
            };

            foreach (var reading in pirSequence)
            {
                // Act
                await MotionController.BroadcastPirData(reading);

                // Assert
                var buffer = new byte[1024];
                var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                var data = JsonSerializer.Deserialize<PirSensorData>(message);

                Assert.NotNull(data);
                Assert.NotEqual(previousState, data.MotionDetected); // Verify state change
                previousState = data.MotionDetected;
            }
        }
        finally
        {
            if (socket.State == WebSocketState.Open)
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
        }
    }
}

// Test startup class for WebSocket testing
public class TestStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app)
    {
        app.UseWebSockets();
        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}
