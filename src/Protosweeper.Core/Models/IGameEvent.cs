namespace Protosweeper.Core.Models;

public interface IGameEvent
{
    DateTime Timestamp { get; }
    string Type { get; }
}