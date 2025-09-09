using System.Text.Json.Serialization;

namespace Protosweeper.Web.Models;

public record CellResponse : GameResponseBase
{
    [JsonPropertyName("type")]
    public override string Type => "cell";
    
    [JsonPropertyName("count")]
    public int Count { get; set; }
    
    [JsonPropertyName("x")]
    public int X { get; set; }
    
    [JsonPropertyName("y")]
    public int Y { get; set; }
}