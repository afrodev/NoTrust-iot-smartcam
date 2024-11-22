using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using CameraBackend.Api.Models;

namespace CameraBackend.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class MotionController : ControllerBase
{
    private static readonly List<WebSocket> _connectedClients = new();
    private static PirSensorData _lastPirReading = new() 
    { 
        MotionDetected = false,
        Timestamp = DateTime.UtcNow
    };
    
    [Route("/motion")]
    public async Task HandleMotion()
    {
        if (!HttpContext.WebSockets.IsWebSocketRequest)
        {
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
        _connectedClients.Add(webSocket);

        try
        {
            // Send the initial state to the new client
            await SendPirData(webSocket, _lastPirReading);

            var buffer = new byte[1024 * 4];
            var receiveResult = await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!receiveResult.CloseStatus.HasValue)
            {
                // In a real implementation, this would be triggered by actual PIR sensor hardware
                await SimulatePirSensorReading(webSocket);
                
                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(
                receiveResult.CloseStatus.Value,
                receiveResult.CloseStatusDescription,
                CancellationToken.None);
        }
        catch (WebSocketException)
        {
            // Handle WebSocket errors
        }
        finally
        {
            _connectedClients.Remove(webSocket);
        }
    }

    private async Task SimulatePirSensorReading(WebSocket webSocket)
    {
        // Simulate PIR sensor behavior with random state changes
        var random = new Random();
        var newReading = new PirSensorData
        {
            Timestamp = DateTime.UtcNow,
            MotionDetected = random.NextDouble() > 0.7 // 30% chance of motion detection
        };

        await SendPirData(webSocket, newReading);
    }

    private static async Task SendPirData(WebSocket webSocket, PirSensorData pirData)
    {
        var json = JsonSerializer.Serialize(pirData);
        var bytes = Encoding.UTF8.GetBytes(json);
        
        if (webSocket.State == WebSocketState.Open)
        {
            await webSocket.SendAsync(
                new ArraySegment<byte>(bytes),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None);
        }
    }

    // Method to broadcast PIR data to all connected clients
    public static async Task BroadcastPirData(PirSensorData pirData)
    {
        _lastPirReading = pirData;
        var json = JsonSerializer.Serialize(pirData);
        var bytes = Encoding.UTF8.GetBytes(json);

        var deadSockets = new List<WebSocket>();

        foreach (var socket in _connectedClients)
        {
            try
            {
                if (socket.State == WebSocketState.Open)
                {
                    await socket.SendAsync(
                        new ArraySegment<byte>(bytes),
                        WebSocketMessageType.Text,
                        true,
                        CancellationToken.None);
                }
                else
                {
                    deadSockets.Add(socket);
                }
            }
            catch
            {
                deadSockets.Add(socket);
            }
        }

        foreach (var socket in deadSockets)
        {
            _connectedClients.Remove(socket);
        }
    }
}
