using System.Runtime.InteropServices;

namespace Protosweeper.Core.Models;

[StructLayout(LayoutKind.Sequential)]
public readonly struct XyPair(int x, int y) : IEquatable<XyPair>
{
    public readonly int X = x;
    public readonly int Y = y;

    public int PixelDistance(XyPair other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

    public static XyPair operator +(XyPair left, XyPair right) => new(left.X + right.X, left.Y + right.Y);
    public static XyPair operator -(XyPair left, XyPair right) => new(left.X - right.X, left.Y - right.Y);
    public static bool operator ==(XyPair left, XyPair right) => left.Equals(right);
    public static bool operator !=(XyPair left, XyPair right) => !(left == right);

    public bool Equals(XyPair other) => X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is XyPair other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);
    public override string ToString() => $"({X}, {Y})";

    public List<XyPair> Neighbours(XyPair bounds, int range = 1)
    {
        var neighbours = new List<XyPair>();

        for (var dx = -range; dx <= range; dx++)
        {
            var x = X + dx;
            for (var dy = -range; dy <= range; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;
                
                var y = Y + dy;
                
                if (x < 0 || y < 0)
                    continue;
                
                if (x >= bounds.X || y >= bounds.Y)
                    continue;
                
                neighbours.Add(new XyPair(x, y));
            }
        }

        return neighbours;
    }
}