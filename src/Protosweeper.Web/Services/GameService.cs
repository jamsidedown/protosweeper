using System.Collections.Concurrent;
using Protosweeper.Core.Models;
using Protosweeper.Web.Models;

namespace Protosweeper.Web.Services;

public class GameService(ILogger<GameService> logger)
{
    private static ConcurrentDictionary<string, GameInPlay> Games = [];

    public GameId New(Difficulty difficulty, int initialX, int initialY, Guid? optionalSeed = null)
    {
        var initialClick = new XyPair(initialX, initialY);
        // var seed = optionalSeed ?? Guid.CreateVersion7();
        var seed = optionalSeed ?? Guid.Parse("019d6342-7c29-79fd-a79a-ab3d7e785d0e");
        var gameBoard = GameBoard.Generate(seed, difficulty, initialClick);
        
        var cts = new CancellationTokenSource();
        var game = new GameInPlay { GameBoard = gameBoard, CancellationTokenSource = cts };

        var id = new GameId(seed, difficulty, initialX, initialY);

        game.GameRunner = Play(id, game, cts.Token);

        return Games.TryAdd(id.ToString(), game) ? id : throw new Exception("Unable to create game");
    }

    public (bool, GameInPlay?) Get(GameId id)
    {
        return Games.TryGetValue(id.ToString(), out var game) ? (true, game) : (false, null);
    }
    
    public async Task Play(GameId gameId, GameInPlay game, CancellationToken token)
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
        catch (OperationCanceledException)
        {
            logger.LogInformation("Game {id} ended due to CancellationToken, stopping {method}", gameId, nameof(Play));
        }
    }

    public async Task Cleanup(Guid clientId, GameId gameId)
    {
        if (Games.TryGetValue(gameId.ToString(), out var game))
        {
            game.ResponseChannelWriters.TryRemove(clientId, out _);

            if (game.ResponseChannelWriters.IsEmpty)
            {
                await game.CancellationTokenSource.CancelAsync();
                game.RequestChannel.Writer.Complete();
                Games.TryRemove(gameId.ToString(), out _);
            }
        }
    }
}