using System.Collections.Concurrent;
using System.Threading.Channels;
using Protosweeper.Core.Models;

namespace Protosweeper.Web.Services;

public class GameService(ILogger<GameService> logger)
{
    private static ConcurrentDictionary<Guid, GameBoard> Games = [];

    public Guid New(Difficulty difficulty, int initialX, int initialY)
    {
        var game = GameBoard.Generate(difficulty, new XyPair(initialX, initialY));
        var id = Guid.CreateVersion7();

        Games.AddOrUpdate(id, _ => game, (_, _) => game);

        return id;
    }

    public (bool, GameBoard?) Get(Guid id)
    {
        return Games.TryGetValue(id, out var game) ? (true, game) : (false, null);
    }
    
    public async Task Play(Guid id, GameBoard game, ChannelReader<IGameRequest> receiver, ChannelWriter<IGameResponse> sender, CancellationToken token)
    {
        try
        {
            var initialClick = new GameRequestClick { Button = "left", X = game.InitialClick.X, Y = game.InitialClick.Y };
            
            logger.LogTrace("Client {id} received initial click {initialClick}", id, initialClick);
            
            foreach (var response in game.Click(initialClick))
                await sender.WriteAsync(response, token);
            
            logger.LogTrace("Client {id} handled initial click", id);
            
            await foreach (var request in receiver.ReadAllAsync(token))
            {
                logger.LogTrace("Client {id} received request {request}", id, request);
                
                var responses = request switch
                {
                    GameRequestClick click => game.Click(click),
                    _ => [],
                };

                foreach (var response in responses)
                {
                    await sender.WriteAsync(response, token);

                    if (response is WinResponse or LoseResponse)
                        game.ReadOnly = true;
                }
                
                logger.LogTrace("Client {id} handled request", id);
            }
        }
        catch (OperationCanceledException _)
        {
            logger.LogInformation("Client {id} disconnected due to CancellationToken, stopping {method}", id, nameof(Play));
        }
    }
}