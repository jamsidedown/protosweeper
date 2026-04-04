using Microsoft.AspNetCore.Mvc;
using Protosweeper.Core;
using Protosweeper.Core.Models;
using Protosweeper.Web.Services;

namespace Protosweeper.Web.Controllers;

[ApiController]
[Route("[controller]")]
public class GameController(GameService gameService, ILogger<GameController> logger) : ControllerBase
{
    [Route("new")]
    [HttpPost]
    public async Task New(string difficulty, int x, int y, CancellationToken token)
    {
        try
        {
            var gameId = gameService.New(Definitions.ParseDifficulty(difficulty), x, y);
            
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}