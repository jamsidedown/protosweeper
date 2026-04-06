using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace Protosweeper.Core.Extensions;

public static class WebsocketExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        { PropertyNameCaseInsensitive = true };
    
    extension(WebSocket websocket)
    {
        private async Task SendBytes(byte[] bytes, CancellationToken token = default)
        {
            await websocket.SendAsync(bytes, WebSocketMessageType.Text, endOfMessage: true, token);
        }

        private async Task SendMessage(string message, CancellationToken token = default)
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            await websocket.SendBytes(bytes, token);
        }

        public async Task SendMessage<T>(T model, CancellationToken token = default)
        {
            var json = JsonSerializer.Serialize(model);
            await websocket.SendMessage(json, token);
        }

        public async Task<string> ReceiveMessage(CancellationToken token = default)
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

        public async Task<T?> ReceiveMessage<T>(CancellationToken token = default)
        {
            var json = await websocket.ReceiveMessage(token);
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        
        public async Task WaitUntilConnected(CancellationToken token = default)
        {
            while (ConnectingStates.Contains(websocket.State))
                await Task.Delay(TimeSpan.FromMilliseconds(10), token);
        }

        public async Task GracefullyClose(CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;
        
            await websocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", token);
        }
    }

    private static readonly HashSet<WebSocketState> ConnectingStates = [WebSocketState.None, WebSocketState.Connecting];
}