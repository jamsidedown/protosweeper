using System.Text.Json.Serialization;

namespace Protosweeper.Web.Models;

public class UnflagResponse : GameResponseBase
{
    [JsonPropertyName("type")]
    public override string Type => "unflag";
    [JsonPropertyName("x")]
    public int X { get; set; }
    [JsonPropertyName("y")]
    public int Y { get; set; }
}