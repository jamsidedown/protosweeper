using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Protosweeper.Web.Extensions;

public static class WebsocketExtensions
{
    public static async Task SendBytes(this WebSocket websocket, byte[] bytes, CancellationToken token = default)
    {
        await websocket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, token);
    }
    
    public static async Task SendMessage(this WebSocket websocket, string message, CancellationToken token = default)
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        await websocket.SendBytes(bytes, token);
    }
    
    public static async Task SendMessage<T>(this WebSocket websocket, T model, CancellationToken token = default)
    {
        var json = JsonSerializer.Serialize(model);
        await websocket.SendMessage(json, token);
    }
    
    public static async Task<string> ReceiveMessage(this WebSocket websocket, CancellationToken token = default)
    {
        var buffers = new StringBuilder();

        while (true)
        {
            var buffer = new byte[1024];
            var response = await websocket.ReceiveAsync(buffer, token);
            var part = Encoding.UTF8.GetString(buffer, 0, response.Count);
            buffers.Append(part);

            if (response.EndOfMessage)
                break;
        }

        return buffers.ToString();
    }
    
    public static async Task<T?> ReceiveMessage<T>(this WebSocket websocket, CancellationToken token = default)
    {
        var json = await websocket.ReceiveMessage(token);
        return JsonSerializer.Deserialize<T>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = false });
    }
    
    private static readonly HashSet<WebSocketState> ConnectingStates = [WebSocketState.None, WebSocketState.Connecting];

    public static async Task WaitUntilConnected(this WebSocket websocket, CancellationToken token = default)
    {
        while (ConnectingStates.Contains(websocket.State))
            await Task.Delay(TimeSpan.FromMilliseconds(10), token);
    }
}