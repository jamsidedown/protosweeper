using System.Text.Json.Serialization;

namespace Protosweeper.Web.Models;

public class WinResponse : GameResponseBase
{
    [JsonPropertyName("type")]
    public override string Type => "win";
}