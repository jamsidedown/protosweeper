using System.Text.Json.Serialization;

namespace Protosweeper.Web.Models;

public record WinResponse : GameResponseBase
{
    [JsonPropertyName("type")]
    public override string Type => "win";
}