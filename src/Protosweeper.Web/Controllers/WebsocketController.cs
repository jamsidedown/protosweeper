using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.AspNetCore.Mvc;
using Protosweeper.Core.Extensions;
using Protosweeper.Core.Models;
using Protosweeper.Web.Models;
using Protosweeper.Web.Services;

namespace Protosweeper.Web.Controllers;

[ApiController]
[Route("ws")]
public class WebsocketController(ILogger<WebsocketController> logger, GameService gameService) : ControllerBase
{
    private static readonly ConcurrentDictionary<Guid, ConcurrentBag<WebsocketState>> Games = new();
    private static readonly ConcurrentDictionary<Guid, WebsocketState> Connections = new();
    private static bool _shutdownRequested = false;

    [Route("{id}")]
    public async Task Get(string id, CancellationToken token)
    {
        var clientId = Guid.CreateVersion7();
        var gameId = GameId.Parse(id);

        try
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                logger.LogTrace("Client {clientId} opening", clientId);
                using var websocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                logger.LogTrace("Client {clientId} accepted", clientId);
                await websocket.WaitUntilConnected(token);
                logger.LogTrace("Client {clientId} fully connected", clientId);

                logger.LogTrace("Getting game {gameId} for client {clientId}", gameId, clientId);
                var (success, game) = gameService.Get(gameId);
                if (!success || game is null)
                {
                    logger.LogWarning("Failed to get game {gameId} for client {clientId}", gameId, clientId);
                    Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                logger.LogTrace("Successfully got game {gameId} for client {clientId}", gameId, clientId);

                var cts = CancellationTokenSource.CreateLinkedTokenSource(token, game.CancellationTokenSource.Token);
                var state = new WebsocketState { Websocket = websocket, Cts = cts };
                Connections.TryAdd(clientId, state);
                logger.LogInformation("Client {clientId} connected to game {gameId}", clientId, gameId);

                var requestsWriter = game.RequestChannel.Writer;
                var responses = Channel.CreateBounded<IGameResponse>(256);
                game.ResponseChannelWriters.TryAdd(clientId, responses.Writer);

                var tasks = new List<Task>
                {
                    HandleRequests(clientId, websocket, requestsWriter, cts.Token),
                    HandleResponses(clientId, websocket, responses.Reader, cts.Token),
                };

                await ReplayGame(game.GameBoard, websocket, responses.Writer, cts.Token);
                logger.LogInformation("Replayed game {gameId} for client {clientId}", gameId, clientId);

                await Task.WhenAny(tasks);
                logger.LogInformation("Client {clientId} ended game {gameId}", clientId, gameId);

                await cts.CancelAsync();
                logger.LogTrace("Cancelled token for client {clientId} and game {gameId}", clientId, gameId);

                await websocket.GracefullyClose(cts.Token);
                logger.LogInformation("Client {clientId} gracefully closed", clientId);
            }
            else
            {
                Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Less shit");
        }
        catch (Exception e)
        {
            Console.WriteLine("Oh shit");
            Console.WriteLine(e);
        }
        
        Connections.TryRemove(clientId, out _);
        await gameService.Cleanup(clientId, gameId);
    }
    
    private async Task ReplayGame(GameBoard game, WebSocket websocket, ChannelWriter<IGameResponse> channel,
        CancellationToken token)
    {
        try
        {
            foreach (var response in game.ReplayResponses())
            {
                if (websocket.State == WebSocketState.Open && !_shutdownRequested && !token.IsCancellationRequested)
                {
                    await SendResponse(response, websocket, token);
                }
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
                logger.LogTrace("Client {id} request {message}", id, message);

                if (message == "")
                    continue;

                var click = JsonSerializer.Deserialize<GameRequestClick>(message);
                logger.LogInformation("Client {id} received {click}", id, click);

                if (click is null)
                    continue;

                await channel.WriteAsync(click, token);
                logger.LogTrace("Client {id} click published", id);
            }
            
            logger.LogInformation("Client {id} disconnected, stopping {method}", id, nameof(HandleRequests));
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Client {id} disconnected due to CancellationToken, stopping {method}", id, nameof(HandleRequests));
        }
    }
    
    private async Task HandleResponses(Guid clientId, WebSocket websocket, ChannelReader<IGameResponse> channel, CancellationToken token)
    {
        try
        {
            while (websocket.State == WebSocketState.Open && !_shutdownRequested &&
                   !token.IsCancellationRequested)
            {
                var response = await channel.ReadAsync(token);
                logger.LogInformation("Client {id} sending {response}", clientId, response);
                await SendResponse(response, websocket, token);
            }
            
            logger.LogInformation("Client {id} disconnected, stopping {method}", clientId, nameof(HandleResponses));
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Client {id} disconnected due to CancellationToken, stopping {method}", clientId, nameof(HandleResponses));
        }
    }

    private async Task SendResponse(IGameResponse response, WebSocket websocket, CancellationToken token)
    {
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

    public static async Task Shutdown()
    {
        _shutdownRequested = true;
        var clients = Connections.ToList();
        
        foreach (var (id, state) in clients)
        {
            var cts = state.Cts;
            try
            {
                await cts.CancelAsync();
                Console.WriteLine($"Client {id} cancelled token");
            }
            catch (Exception)
            {
                Console.WriteLine($"Client {id} failed to cancel token");
            }
        }
    }
}