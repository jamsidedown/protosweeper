using System.Text.Json.Serialization;

namespace Protosweeper.Web.Models;

public class NewGameDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }
    
    [JsonPropertyName("difficulty")]
    public string Difficulty { get; set; }
}