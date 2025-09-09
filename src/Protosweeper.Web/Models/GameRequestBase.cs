using System.Text.Json.Serialization;

namespace Protosweeper.Web.Models;

public abstract record GameRequestBase : IGameRequest
{
    [JsonPropertyName("type")]
    public abstract string Type { get; }
}