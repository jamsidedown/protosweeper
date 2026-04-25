using System.Text.Json.Serialization;

namespace Protosweeper.Core.Models;

public record LoseResponse : GameResponseBase
{
    [JsonPropertyName("type")]
    public override string Type => "lose";
    
    [JsonPropertyName("seed")]
    public required Guid SeedId { get; init; }
}