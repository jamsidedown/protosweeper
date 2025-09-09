using System.Text.Json.Serialization;

namespace Protosweeper.Web.Models;

public record FlagResponse : GameResponseBase
{
    [JsonPropertyName("type")]
    public override string Type => "flag";
    
    [JsonPropertyName("x")]
    public int X { get; set; }
    
    [JsonPropertyName("y")]
    public int Y { get; set; }
}