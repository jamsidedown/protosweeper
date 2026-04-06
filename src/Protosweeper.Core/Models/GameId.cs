using System.Runtime.InteropServices;

namespace Protosweeper.Core.Models;

[StructLayout(LayoutKind.Sequential)]
public struct GameId(Guid seed, Difficulty difficulty, int x, int y)
{
    public Guid Seed { get; } = seed;
    public Difficulty Difficulty { get; } = difficulty;
    public byte InitialX { get; } = (byte)x;
    public byte InitialY { get; } = (byte)y;

    public override string ToString()
    {
        var bytes = new byte[Marshal.SizeOf<GameId>()];
        var span = MemoryMarshal.CreateReadOnlySpan(ref this, 1);
        MemoryMarshal.AsBytes(span).CopyTo(bytes);

        var s = Convert.ToBase64String(bytes);
        return Uri.EscapeDataString(s);
    }

    public static GameId Parse(string id)
    {
        var s = Uri.UnescapeDataString(id);
        var bytes = Convert.FromBase64String(s);
        return MemoryMarshal.Cast<byte, GameId>(bytes)[0];
    }
}