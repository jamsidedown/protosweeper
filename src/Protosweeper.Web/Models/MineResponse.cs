using System.Text.Json.Serialization;

namespace Protosweeper.Web.Models;

public class MineResponse : GameResponseBase
{
    [JsonPropertyName("type")]
    public override string Type => "mine";
    [JsonPropertyName("x")]
    public int X { get; set; }
    [JsonPropertyName("y")]
    public int Y { get; set; }
}