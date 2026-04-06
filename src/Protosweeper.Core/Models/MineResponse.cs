using System.Text.Json.Serialization;

namespace Protosweeper.Core.Models;

public record MineResponse : GameResponseBase
{
    [JsonPropertyName("type")]
    public override string Type => "mine";
    
    [JsonPropertyName("x")]
    public int X { get; set; }
    
    [JsonPropertyName("y")]
    public int Y { get; set; }
}