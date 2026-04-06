using System.Text.Json.Serialization;

namespace Protosweeper.Core.Models;

public abstract record GameResponseBase : IGameResponse
{
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; } = DateTime.Now;

    [JsonPropertyName("type")]
    public abstract string Type { get; }
}