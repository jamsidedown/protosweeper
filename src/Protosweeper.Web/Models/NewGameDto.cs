using System.Text.Json.Serialization;

namespace Protosweeper.Web.Models;

public class NewGameDto
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }
    
    [JsonPropertyName("difficulty")]
    public required string Difficulty { get; set; }
}