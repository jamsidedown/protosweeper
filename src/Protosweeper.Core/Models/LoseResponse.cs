using System.Text.Json.Serialization;

namespace Protosweeper.Core.Models;

public record LoseResponse : GameResponseBase
{
    [JsonPropertyName("type")]
    public override string Type => "lose";
}