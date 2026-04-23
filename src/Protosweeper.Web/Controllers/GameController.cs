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
    public async Task<NewGameDto?> New(string difficulty, int x, int y)
    {
        try
        {
            var gameId = await gameService.New(Definitions.ParseDifficulty(difficulty), x, y);
            return new NewGameDto
            {
                Id = gameId.ToString(),
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
}