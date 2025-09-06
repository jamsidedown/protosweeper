using System.Collections.Concurrent;
using System.Net.WebSockets;
using Microsoft.AspNetCore.Mvc;
using Protosweeper.Web.Extensions;
using Protosweeper.Web.Models;

namespace Protosweeper.Web.Controllers;

[ApiController]
[Route("ws")]
public class WebsocketController(ILogger<WebsocketController> logger) : ControllerBase
{
    private static readonly ConcurrentDictionary<Guid, WebSocket> _connections = new();
    
    public async Task Get(CancellationToken token)
    {
        var id = Guid.NewGuid();

        try
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var websocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await websocket.WaitUntilConnected(token);
                _connections.TryAdd(id, websocket);

                GameBoard? game = null;
                
                while (websocket.State == WebSocketState.Open && !token.IsCancellationRequested)
                {
                    
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}