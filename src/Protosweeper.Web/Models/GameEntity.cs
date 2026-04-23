using Protosweeper.Core.Models;

namespace Protosweeper.Web.Models;

// [StructLayout(LayoutKind.Sequential)]
public class GameEntity
{
    public Guid Id { get; init; }
    public Guid Seed { get; init; }
    public Difficulty Difficulty { get; init; }
    public int InitialX { get; init; }
    public int InitialY { get; init; }

    // public override string ToString()
    // {
    //     var bytes = new byte[Marshal.SizeOf<GameId>()];
    //     var span = MemoryMarshal.CreateReadOnlySpan(ref this, 1);
    //     MemoryMarshal.AsBytes(span).CopyTo(bytes);
    //
    //     var s = Convert.ToBase64String(bytes);
    //     return Uri.EscapeDataString(s);
    // }

    // public static GameId Parse(string id)
    // {
    //     var s = Uri.UnescapeDataString(id);
    //     var bytes = Convert.FromBase64String(s);
    //     return MemoryMarshal.Cast<byte, GameId>(bytes)[0];
    // }
}