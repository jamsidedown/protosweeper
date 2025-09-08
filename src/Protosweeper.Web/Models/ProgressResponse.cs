using System.Text.Json.Serialization;

namespace Protosweeper.Web.Models;

public class ProgressResponse : GameResponseBase
{
    [JsonPropertyName("type")]
    public override string Type => "progress";
    
    [JsonPropertyName("flagged")]
    public string Flagged { get; set; }
}