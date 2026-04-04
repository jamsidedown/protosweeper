using System.Text.Json.Serialization;

namespace Protosweeper.Core.Models;

public record UnflagResponse : GameResponseBase
{
    [JsonPropertyName("type")]
    public override string Type => "unflag";
    
    [JsonPropertyName("x")]
    public int X { get; set; }
    
    [JsonPropertyName("y")]
    public int Y { get; set; }
}