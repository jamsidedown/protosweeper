using System.Threading.Channels;
using Protosweeper.Web.Models;

namespace Protosweeper.Web.Services;

public class GameService
{
    public async Task Play(Guid id, GameBoard game, ChannelReader<IGameRequest> receiver, ChannelWriter<IGameResponse> sender, CancellationTokenSource cts)
    {
        var token = cts.Token;

        try
        {
            var initialClick = new GameRequestClick { Button = "left", X = game.InitialClick.X, Y = game.InitialClick.Y };
            foreach (var response in game.Click(initialClick))
                await sender.WriteAsync(response, token);
            
            await foreach (var request in receiver.ReadAllAsync(token))
            {
                var responses = request switch
                {
                    GameRequestClick click => game.Click(click),
                    _ => [],
                };

                foreach (var response in responses)
                    await sender.WriteAsync(response, token);
            }
        }
        catch (OperationCanceledException _)
        {
            Console.WriteLine("Cancellation caught in GameService.Play");
        }
    }
}