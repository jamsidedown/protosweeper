using System.Net.WebSockets;

namespace Protosweeper.Core.Models;

public class WebsocketState
{
    public WebSocket Websocket { get; set; }
    public CancellationTokenSource Cts { get; set; }
}