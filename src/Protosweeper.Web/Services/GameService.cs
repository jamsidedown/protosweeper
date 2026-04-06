using System.Collections.Concurrent;
using System.Threading.Channels;
using Protosweeper.Core.Models;
using Protosweeper.Web.Models;

namespace Protosweeper.Web.Services;

public class GameService(ILogger<GameService> logger)
{
    private static ConcurrentDictionary<Guid, GameInPlay> Games = [];

    public Guid New(Difficulty difficulty, int initialX, int initialY)
    {
        var gameBoard = GameBoard.Generate(difficulty, new XyPair(initialX, initialY));
        var id = Guid.CreateVersion7();
        var cts = new CancellationTokenSource();
        var game = new GameInPlay { GameBoard = gameBoard, CancellationTokenSource = cts };

        game.GameRunner = Play(id, game, cts.Token);

        return Games.TryAdd(id, game) ? id : throw new Exception("Unable to create game");
    }

    public (bool, GameInPlay?) Get(Guid id)
    {
        return Games.TryGetValue(id, out var game) ? (true, game) : (false, null);
    }
    
    public async Task Play(Guid gameId, GameInPlay game, CancellationToken token)
    {
        var gameBoard = game.GameBoard;
        var receiver = game.RequestChannel.Reader;
        
        try
        {
            var initialClick = new GameRequestClick { Button = "left", X = gameBoard.InitialClick.X, Y = gameBoard.InitialClick.Y };
            
            logger.LogTrace("Game {id} received initial click {initialClick}", gameId, initialClick);
            
            await foreach (var response in gameBoard.Click(initialClick, token))
            foreach (var sender in game.ResponseChannelWriters.Values.ToList())
            {
                try
                {
                    await sender.WriteAsync(response, token);
                }
                catch (Exception e)
                {
                    logger.LogWarning("{}", e.Message);
                }
            }
            
            logger.LogTrace("Game {id} handled initial click", gameId);
            
            await foreach (var request in receiver.ReadAllAsync(token))
            {
                logger.LogTrace("Game {id} received request {request}", gameId, request);

                if (request is GameRequestClick click)
                {
                    await foreach (var response in gameBoard.Click(click, token))
                    foreach (var sender in game.ResponseChannelWriters.Values.ToList())
                    {
                        try
                        {
                            await sender.WriteAsync(response, token);
                        }
                        catch (Exception e)
                        {
                            logger.LogWarning("{}", e.Message);
                        }

                        if (response is WinResponse or LoseResponse)
                            gameBoard.ReadOnly = true;
                    }
                }
                
                logger.LogTrace("Game {id} handled request", gameId);
            }
        }
        catch (OperationCanceledException _)
        {
            logger.LogInformation("Game {id} ended due to CancellationToken, stopping {method}", gameId, nameof(Play));
        }
    }

    public async Task Cleanup(Guid clientId, Guid gameId)
    {
        if (Games.TryGetValue(gameId, out var game))
        {
            game.ResponseChannelWriters.TryRemove(clientId, out _);

            if (game.ResponseChannelWriters.IsEmpty)
            {
                await game.CancellationTokenSource.CancelAsync();
                game.RequestChannel.Writer.Complete();
                Games.TryRemove(gameId, out _);
            }
        }
    }
}