using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using Protosweeper.Web.Extensions;
using Protosweeper.Web.Models;

namespace Protosweeper.Web.Controllers;

[ApiController]
[Route("ws")]
public class WebsocketController(ILogger<WebsocketController> logger) : ControllerBase
{
    private static readonly ConcurrentDictionary<Guid, WebsocketState> _connections = new();
    private static bool _shutdownRequested = false;
    
    public async Task Get(string difficulty, int x, int y, CancellationToken token)
    {
        var id = Guid.NewGuid();

        try
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var websocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await websocket.WaitUntilConnected(token);
                var game = GameBoard.Generate(Definitions.ParseDifficulty(difficulty), new XyPair(x, y));

                var state = new WebsocketState
                {
                    Websocket = websocket,
                    Game = game,
                    Cts = CancellationTokenSource.CreateLinkedTokenSource(token),
                };
                
                _connections.TryAdd(id, state);

                Task.WaitAll(state.Game.Run(state.Cts.Token),
                    HandleRequests(state, state.Cts.Token),
                    HandleResponses(state, state.Cts.Token));
            }
            else
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    private async Task HandleRequests(WebsocketState state, CancellationToken token)
    {
        var channelWriter = state.Game.Requests.Writer;

        try
        {
            while (state.Websocket.State == WebSocketState.Open && !_shutdownRequested &&
                   !token.IsCancellationRequested)
            {
                var message = await state.Websocket.ReceiveMessage(token);

                if (message == "")
                    continue;

                var click = JsonSerializer.Deserialize<GameRequestClick>(message);

                if (click is null)
                    continue;

                await channelWriter.WriteAsync(click, token);
            }
        }
        catch (OperationCanceledException _)
        {
            Console.WriteLine("Cancellation caught in HandleRequests");
        }
    }
    
    private async Task HandleResponses(WebsocketState state, CancellationToken token)
    {
        var channelReader = state.Game.Responses.Reader;

        try
        {
            while (state.Websocket.State == WebSocketState.Open && !_shutdownRequested &&
                   !token.IsCancellationRequested)
            {
                var response = await channelReader.ReadAsync(token);

                var task = response switch
                {
                    MineResponse res => state.Websocket.SendMessage(res, token),
                    CellResponse res => state.Websocket.SendMessage(res, token),
                    FlagResponse res => state.Websocket.SendMessage(res, token),
                    UnflagResponse res => state.Websocket.SendMessage(res, token),
                    ProgressResponse res => state.Websocket.SendMessage(res, token),
                    WinResponse res => state.Websocket.SendMessage(res, token),
                    LoseResponse res => state.Websocket.SendMessage(res, token),
                    _ => Task.FromResult(false),
                };

                await task;
            }
        }
        catch (OperationCanceledException _)
        {
            Console.WriteLine("Cancellation caught in HandleResponses");
        }
        
    }

    public static async Task Shutdown()
    {
        _shutdownRequested = true;
        var clients = _connections.ToList();

        foreach (var pair in clients)
        {
            var id = pair.Key;
            var state = pair.Value;
            
            try
            {
                await state.Cts.CancelAsync();
                await state.Websocket.SendMessage("Server shutting down");
                await state.Websocket.GracefullyClose(CancellationToken.None);
                Console.WriteLine($"Disconnected {id}");
            }
            catch
            {
                Console.WriteLine($"Failed to gracefully disconnect {id}");
            }
        }
    }
}