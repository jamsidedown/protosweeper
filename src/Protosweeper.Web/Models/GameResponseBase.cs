using System.Text.Json.Serialization;

namespace Protosweeper.Web.Models;

public abstract class GameResponseBase : IGameResponse
{
    [JsonPropertyName("type")]
    public abstract string Type { get; }
}