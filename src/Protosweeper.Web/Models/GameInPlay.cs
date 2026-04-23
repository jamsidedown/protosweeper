using System.Collections.Concurrent;
using System.Threading.Channels;
using Protosweeper.Core.Models;

namespace Protosweeper.Web.Models;

public class GameInPlay
{
    public required Guid GameId { get; init; }
    public required GameBoard GameBoard { get; init; }
    public required GameEntity GameEntity { get; init; }
    public required CancellationTokenSource CancellationTokenSource { get; init; }
    public Task? GameRunner { get; set; }
    public Channel<IGameRequest> RequestChannel { get; } = Channel.CreateBounded<IGameRequest>(256);
    public ConcurrentDictionary<Guid, ChannelWriter<IGameResponse>> ResponseChannelWriters { get; } = [];
}
