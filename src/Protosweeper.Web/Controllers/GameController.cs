using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Protosweeper.Core;
using Protosweeper.Core.Models;
using Protosweeper.Web.Models;
using Protosweeper.Web.Services;

namespace Protosweeper.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController(GameService gameService, ILogger<GameController> logger) : ControllerBase
{
    [ValidateAntiForgeryToken]
    [Route("new")]
    [HttpPost]
    public async Task<NewGameDto?> New(string difficulty, int x, int y, CancellationToken token)
    {
        try
        {
            var gameId = await gameService.New(Definitions.ParseDifficulty(difficulty), x, y, token);
            
            return new NewGameDto
            {
                Id = gameId.ToString()!,
                Difficulty = difficulty,
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return null;
        }
    }
    
    [ValidateAntiForgeryToken]
    [Route("practice")]
    [HttpPost]
    public async Task<NewGameDto?> Practice(string seed, CancellationToken token = default)
    {
        try
        {
            var game = await gameService.Practice(Guid.Parse(seed), token);

            if (game is null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                return null;
            }

            var (gameId, difficulty) = game.Value;
                
            return new NewGameDto
            {
                Id = gameId.ToString(),
                Difficulty = Definitions.StringifyDifficulty(difficulty),
            };
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return null;
        }
    }
}