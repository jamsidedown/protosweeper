using System.Text.Json.Serialization;

namespace Protosweeper.Web.Models;

public record LoseResponse : GameResponseBase
{
    [JsonPropertyName("type")]
    public override string Type => "lose";
}