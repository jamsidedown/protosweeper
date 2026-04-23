using System.Collections.Concurrent;
using Protosweeper.Core.Models;
using Protosweeper.Web.Data;
using Protosweeper.Web.Models;

namespace Protosweeper.Web.Services;

public class GameService(GameRepository repo, ILogger<GameService> logger)
{
    private static ConcurrentDictionary<Guid, GameInPlay> Games = [];

    public async Task<Guid> New(Difficulty difficulty, int initialX, int initialY, CancellationToken token = default)
    {
        var initialClick = new XyPair(initialX, initialY);

        var gameId = Guid.CreateVersion7();
        var seedId =  Guid.CreateVersion7();
        var seed = Guid.NewGuid();
        // var seed = Guid.Parse("019d6342-7c29-79fd-a79a-ab3d7e785d0e");
        var gameBoard = GameBoard.Generate(seed, difficulty, initialClick);
        
        var gameEntity = new GameEntity
        {
            Id = seedId,
            Seed = seed,
            Difficulty = difficulty,
            InitialX = initialX,
            InitialY = initialY,
        };
        await repo.Create(gameEntity, token);
        
        var cts = new CancellationTokenSource();
        var game = new GameInPlay
        {
            GameId = gameId,
            GameBoard = gameBoard,
            GameEntity = gameEntity,
            CancellationTokenSource = cts,
        };

        game.GameRunner = Play(gameEntity, game, cts.Token);

        return Games.TryAdd(gameId, game) ? gameId : throw new Exception("Unable to create game");
    }

    public (bool, GameInPlay?) Get(Guid gameId)
    {
        return Games.TryGetValue(gameId, out var game) ? (true, game) : (false, null);
    }
    
    public async Task Play(GameEntity gameEntity, GameInPlay game, CancellationToken token)
    {
        var gameBoard = game.GameBoard;
        var receiver = game.RequestChannel.Reader;
        
        try
        {
            var initialClick = new GameRequestClick { Button = "left", X = gameBoard.InitialClick.X, Y = gameBoard.InitialClick.Y };
            
            logger.LogTrace("Game {id} received initial click {initialClick}", gameEntity, initialClick);
            
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
            
            logger.LogTrace("Game {id} handled initial click", gameEntity);
            
            await foreach (var request in receiver.ReadAllAsync(token))
            {
                logger.LogTrace("Game {id} received request {request}", gameEntity, request);

                if (request is GameRequestClick click)
                {
                    await foreach (var response in gameBoard.Click(click, token))
                    {
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

                        if (response is WinResponse or LoseResponse)
                        {
                            gameBoard.ReadOnly = true;
                            game.RequestChannel.Writer.Complete();
                        }
                    }

                    if (gameBoard.ReadOnly)
                    {
                        await KillGame(game);
                    }
                }
                
                logger.LogTrace("Game {id} handled request", gameEntity);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Game {id} ended due to CancellationToken, stopping {method}", gameEntity, nameof(Play));
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
                game.RequestChannel.Writer.TryComplete();
                Games.TryRemove(gameId, out _);
            }
        }
    }

    private async Task KillGame(GameInPlay game)
    {
        await Task.Delay(TimeSpan.FromSeconds(10));

        foreach (var channelWriter in game.ResponseChannelWriters.Values.ToList())
        {
            channelWriter.TryComplete();
        }
    }
}