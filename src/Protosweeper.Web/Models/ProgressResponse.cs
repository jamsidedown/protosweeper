using System.Text.Json.Serialization;

namespace Protosweeper.Web.Models;

public record ProgressResponse : GameResponseBase
{
    [JsonPropertyName("type")]
    public override string Type => "progress";
    
    [JsonPropertyName("flagged")]
    public string Flagged { get; set; }
}