using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Protosweeper.Core;
using Protosweeper.Web.Controllers;

namespace Protosweeper.Web.Pages;

public class MinesModel(GameController gameController) : PageModel
{
    public string Difficulty = "Beginner";
    public string RestUrl = "http://localhost:5171/game/new";
    public string WsUrl = "ws://localhost:5171/ws";
    public string GameId = "";

    public int Height = 0;
    public int Width = 0;

    private readonly TextInfo _textInfo = CultureInfo.CurrentCulture.TextInfo;
    
    public async Task OnGet(string difficulty, string? seed, CancellationToken token = default)
    {
        if (!string.IsNullOrEmpty(seed))
        {
            var game = await gameController.Practice(seed, token);

            if (game is null)
            {
                Response.StatusCode = StatusCodes.Status404NotFound;
                throw new ArgumentException("Seed {seed} doesn't exist", seed);
            }

            GameId = game.Id;
            Difficulty = game.Difficulty;
        }
        else
        {
            Difficulty = _textInfo.ToTitleCase(difficulty);
        }
        
        var parsedDifficulty = Definitions.ParseDifficulty(Difficulty);
        var dimensions = Definitions.GetDimensions(parsedDifficulty);
        Width = dimensions.X;
        Height = dimensions.Y;
    }
}