using System.Net.WebSockets;

namespace Protosweeper.Web.Models;

public class WebsocketState
{
    public WebSocket Websocket { get; init; }
    public GameBoard Game { get; set; }
    public CancellationTokenSource Cts { get; set; }
}