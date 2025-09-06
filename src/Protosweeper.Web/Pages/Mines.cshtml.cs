using System.Globalization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Protosweeper.Web.Pages;

public class MinesModel : PageModel
{
    public string Difficulty = "Beginner";
    public string WsUrl = "ws://localhost:5171/ws";

    public int Height = 0;
    public int Width = 0;

    private readonly TextInfo _textInfo = CultureInfo.CurrentCulture.TextInfo;
    
    public void OnGet(string difficulty)
    {
        Difficulty = _textInfo.ToTitleCase(difficulty);
        var parsedDifficulty = Definitions.ParseDifficulty(difficulty);
        var dimensions = Definitions.GetDimensions(parsedDifficulty);
        Width = dimensions.X;
        Height = dimensions.Y;
    }
}