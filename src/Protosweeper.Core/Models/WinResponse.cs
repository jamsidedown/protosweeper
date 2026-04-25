using System.Text.Json.Serialization;

namespace Protosweeper.Core.Models;

public record WinResponse : GameResponseBase
{
    [JsonPropertyName("type")]
    public override string Type => "win";
    
    [JsonPropertyName("seed")]
    public required Guid SeedId { get; init; }
}