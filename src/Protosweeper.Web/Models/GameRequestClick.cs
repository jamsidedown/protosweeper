using System.Text.Json.Serialization;

namespace Protosweeper.Web.Models;

public class GameRequestClick : GameRequestBase
{
    [JsonPropertyName("type")]
    public override string Type => "click";
    [JsonPropertyName("button")]
    public string Button { get; set; }
    [JsonPropertyName("x")]
    public int X { get; set; }
    [JsonPropertyName("y")]
    public int Y { get; set; }
}