using System.Text.Json.Serialization;

namespace Protosweeper.Web.Models;

public abstract record GameResponseBase : IGameResponse
{
    [JsonPropertyName("type")]
    public abstract string Type { get; }
}