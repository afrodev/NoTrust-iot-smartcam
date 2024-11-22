using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using CameraBackend.Api.Controllers;
using CameraBackend.Api.Models;
using NSubstitute;

namespace CameraBackend.Api.Tests.Controllers;

public class MotionControllerTests : IDisposable
{
    private readonly MotionController _controller;
    private readonly HttpContext _httpContext;
    private readonly List<TestWebSocket> _testWebSockets;
    private readonly WebSocketManager _webSocketManager;

    public MotionControllerTests()
    {
        _controller = new MotionController();
        _httpContext = Substitute.For<HttpContext>();
        _webSocketManager = Substitute.For<WebSocketManager>();
        _httpContext.WebSockets.Returns(_webSocketManager);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = _httpContext
        };
        _testWebSockets = new List<TestWebSocket>();
    }

    public void Dispose()
    {
        foreach (var webSocket in _testWebSockets)
        {
            webSocket.Dispose();
        }
    }

    [Fact]
    public async Task HandleMotion_ReturnsStatus400_WhenNotWebSocketRequest()
    {
        // Arrange
        _webSocketManager.IsWebSocketRequest.Returns(false);
        var response = Substitute.For<HttpResponse>();
        _httpContext.Response.Returns(response);

        // Act
        await _controller.HandleMotion();

        // Assert
        response.Received(1).StatusCode = StatusCodes.Status400BadRequest;
    }

    [Fact]
    public async Task BroadcastMotionDetection_SendsToAllConnectedClients()
    {
        // Arrange
        _webSocketManager.IsWebSocketRequest.Returns(true);
        var testWebSocket = new TestWebSocket();
        _testWebSockets.Add(testWebSocket);
        
        _webSocketManager.AcceptWebSocketAsync().Returns(testWebSocket);

        // Act
        var handleMotionTask = _controller.HandleMotion();
        
        // Simulate some time for the websocket to connect and receive initial state
        await Task.Delay(100);

        // Assert
        var receivedMessages = testWebSocket.ReceivedMessages;
        Assert.NotEmpty(receivedMessages);
        
        var firstMessage = JsonSerializer.Deserialize<PirSensorData>(
            Encoding.UTF8.GetString(receivedMessages[0]));
        Assert.NotNull(firstMessage);
        Assert.False(firstMessage.MotionDetected);
    }

    [Fact]
    public async Task HandleMotion_HandlesMultipleClients()
    {
        // Arrange
        _webSocketManager.IsWebSocketRequest.Returns(true);
        var testWebSocket1 = new TestWebSocket();
        var testWebSocket2 = new TestWebSocket();
        _testWebSockets.AddRange(new[] { testWebSocket1, testWebSocket2 });

        // Setup sequential returns for multiple calls
        _webSocketManager.AcceptWebSocketAsync()
            .Returns(
                testWebSocket1,
                testWebSocket2);

        // Act
        var client1Task = _controller.HandleMotion();
        var client2Task = _controller.HandleMotion();

        // Wait for both clients to connect and receive initial state
        await Task.Delay(100);

        // Assert
        Assert.NotEmpty(testWebSocket1.ReceivedMessages);
        Assert.NotEmpty(testWebSocket2.ReceivedMessages);

        // Verify both clients received the same initial state
        var message1 = JsonSerializer.Deserialize<PirSensorData>(
            Encoding.UTF8.GetString(testWebSocket1.ReceivedMessages[0]));
        var message2 = JsonSerializer.Deserialize<PirSensorData>(
            Encoding.UTF8.GetString(testWebSocket2.ReceivedMessages[0]));

        Assert.Equal(message1?.MotionDetected, message2?.MotionDetected);
        Assert.Equal(message1?.Timestamp, message2?.Timestamp);
    }
}

// Test helper class to mock WebSocket behavior
public class TestWebSocket : WebSocket
{
    private readonly List<byte[]> _receivedMessages = new();
    private WebSocketState _state = WebSocketState.Open;
    private bool _isDisposed;

    public IReadOnlyList<byte[]> ReceivedMessages => _receivedMessages;

    public override WebSocketState State => _state;

    public override string? SubProtocol => null;

    public override WebSocketCloseStatus? CloseStatus => _state == WebSocketState.Closed ? WebSocketCloseStatus.NormalClosure : null;

    public override string? CloseStatusDescription => _state == WebSocketState.Closed ? "Normal Closure" : null;

    public override void Abort() => _state = WebSocketState.Aborted;

    public override async Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
    {
        _state = WebSocketState.Closed;
        await Task.CompletedTask;
    }

    public override async Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
    {
        _state = WebSocketState.CloseSent;
        await Task.CompletedTask;
    }

    public override void Dispose()
    {
        if (!_isDisposed)
        {
            _state = WebSocketState.Closed;
            _isDisposed = true;
        }
    }

    public override async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
    {
        var messageBytes = new byte[buffer.Count];
        Buffer.BlockCopy(buffer.Array!, buffer.Offset, messageBytes, 0, buffer.Count);
        _receivedMessages.Add(messageBytes);
        await Task.CompletedTask;
    }

    public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        // Simulate keeping the connection open for a while then closing
        await Task.Delay(500, cancellationToken);
        return new WebSocketReceiveResult(0, WebSocketMessageType.Close, true);
    }
}
