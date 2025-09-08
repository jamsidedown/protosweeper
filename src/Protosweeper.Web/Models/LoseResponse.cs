using System.Text.Json.Serialization;

namespace Protosweeper.Web.Models;

public class LoseResponse : GameResponseBase
{
    [JsonPropertyName("type")]
    public override string Type => "lose";
}