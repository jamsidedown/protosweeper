namespace Protosweeper.Web.Models;

public readonly struct XyPair(int x, int y) : IEquatable<XyPair>
{
    public readonly int X = x;
    public readonly int Y = y;

    public static XyPair operator +(XyPair left, XyPair right) => new(left.X + right.X, left.Y + right.Y);
    public static XyPair operator -(XyPair left, XyPair right) => new(left.X - right.X, left.Y - right.Y);
    public static bool operator ==(XyPair left, XyPair right) => left.Equals(right);
    public static bool operator !=(XyPair left, XyPair right) => !(left == right);

    public bool Equals(XyPair other) => X == other.X && Y == other.Y;
    public override bool Equals(object? obj) => obj is XyPair other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(X, Y);

}