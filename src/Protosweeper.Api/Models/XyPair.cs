namespace Protosweeper.Api.Models;

public struct XyPair(int x, int y) : IEquatable<XyPair>
{
    public int X = x;
    public int Y = y;

    public static XyPair operator +(XyPair left, XyPair right) =>
        new XyPair(left.X + right.X, left.Y + right.Y);

    public static XyPair operator -(XyPair left, XyPair right) =>
        new XyPair(left.X - right.X, left.Y - right.Y);

    public bool Equals(XyPair other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        return obj is XyPair other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}