using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using Protosweeper.Web.Extensions;
using Protosweeper.Web.Models;
using Protosweeper.Web.Services;

namespace Protosweeper.Web.Controllers;

[ApiController]
[Route("ws")]
public class WebsocketController(ILogger<WebsocketController> logger, GameService gameService) : ControllerBase
{
    private static readonly ConcurrentDictionary<Guid, WebSocket> _connections = new();
    private static bool _shutdownRequested = false;
    
    public async Task Get(string difficulty, int x, int y, CancellationToken token)
    {
        var id = Guid.NewGuid();

        try
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                logger.LogTrace("Client {id} opening", id);
                using var websocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                logger.LogTrace("Client {id} accepted", id);
                await websocket.WaitUntilConnected(token);
                logger.LogTrace("Client {id} fully connected", id);
                
                var game = GameBoard.Generate(Definitions.ParseDifficulty(difficulty), new XyPair(x, y));
                _connections.TryAdd(id, websocket);
                logger.LogInformation("Client {id} connected with first click at ({x}, {y})", id, x, y);
                
                var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                var requests = Channel.CreateBounded<IGameRequest>(256);
                var responses = Channel.CreateBounded<IGameResponse>(256);

                var tasks = new List<Task>
                {
                    HandleRequests(id, websocket, requests.Writer, cts.Token),
                    HandleResponses(id, websocket, responses.Reader, cts.Token),
                    gameService.Play(id, game, requests.Reader, responses.Writer, cts),
                };
                
                logger.LogInformation("Client {id} {difficulty} game started", id, difficulty);
                await Task.WhenAny(tasks);
                
                logger.LogInformation("Client {id} game ended", id);

                await cts.CancelAsync();
                logger.LogTrace("Client {id} cancelled token", id);
                
                await websocket.GracefullyClose(cts.Token);
                logger.LogInformation("Client {id} gracefully closed", id);
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

    private async Task HandleRequests(Guid id, WebSocket websocket, ChannelWriter<IGameRequest> channel, CancellationToken token)
    {
        try
        {
            while (websocket.State == WebSocketState.Open && !_shutdownRequested &&
                   !token.IsCancellationRequested)
            {
                var message = await websocket.ReceiveMessage(token);
                logger.LogInformation("Client {id} request {message}", id, message);

                if (message == "")
                    continue;

                var click = JsonSerializer.Deserialize<GameRequestClick>(message);
                logger.LogTrace("Client {id} click parsed {click}", id, click);

                if (click is null)
                    continue;

                await channel.WriteAsync(click, token);
                logger.LogTrace("Client {id} click published", id);
            }
            
            logger.LogInformation("Client {id} disconnected, stopping {method}", id, nameof(HandleRequests));
        }
        catch (OperationCanceledException _)
        {
            logger.LogInformation("Client {id} disconnected due to CancellationToken, stopping {method}", id, nameof(HandleRequests));
        }
    }
    
    private async Task HandleResponses(Guid id, WebSocket websocket, ChannelReader<IGameResponse> channel, CancellationToken token)
    {
        try
        {
            while (websocket.State == WebSocketState.Open && !_shutdownRequested &&
                   !token.IsCancellationRequested)
            {
                var response = await channel.ReadAsync(token);

                var task = response switch
                {
                    MineResponse res => websocket.SendMessage(res, token),
                    CellResponse res => websocket.SendMessage(res, token),
                    FlagResponse res => websocket.SendMessage(res, token),
                    UnflagResponse res => websocket.SendMessage(res, token),
                    ProgressResponse res => websocket.SendMessage(res, token),
                    WinResponse res => websocket.SendMessage(res, token),
                    LoseResponse res => websocket.SendMessage(res, token),
                    _ => Task.FromResult(false),
                };

                await task;
            }
            
            logger.LogInformation("Client {id} disconnected, stopping {method}", id, nameof(HandleResponses));
        }
        catch (OperationCanceledException _)
        {
            logger.LogInformation("Client {id} disconnected due to CancellationToken, stopping {method}", id, nameof(HandleResponses));
        }
        
    }

    public static async Task Shutdown()
    {
        _shutdownRequested = true;
        var clients = _connections.ToList();

        foreach (var (id, websocket) in clients)
        {
            try
            {
                await websocket.SendMessage("Server shutting down");
                await websocket.GracefullyClose(CancellationToken.None);
                Console.WriteLine($"Client {id} disconnected");
            }
            catch
            {
                Console.WriteLine($"Client {id} failed to disconnect");
            }
        }
    }
}