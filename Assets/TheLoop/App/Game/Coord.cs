using System;

namespace TheLoop.Game
{
    /// <summary>An integer grid coordinate on the run board (XRC-101). Value type, hashable.</summary>
    public readonly struct Coord : IEquatable<Coord>
    {
        public readonly int X;
        public readonly int Y;

        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int ManhattanTo(Coord other) => Math.Abs(X - other.X) + Math.Abs(Y - other.Y);

        public bool Equals(Coord other) => X == other.X && Y == other.Y;
        public override bool Equals(object obj) => obj is Coord c && Equals(c);
        public override int GetHashCode() => (X * 397) ^ Y;
        public override string ToString() => $"({X},{Y})";

        public static bool operator ==(Coord a, Coord b) => a.Equals(b);
        public static bool operator !=(Coord a, Coord b) => !a.Equals(b);
    }
}
