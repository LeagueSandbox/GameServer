using System;

namespace RoyT.AStar
{
    /// <summary>
    /// A 2D position structure
    /// </summary>
    public struct Position : IEquatable<Position>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">the x-position</param>
        /// <param name="y">the y-position</param>
        public Position(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        /// <summary>
        /// X-position
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Y-position
        /// </summary>
        public int Y { get; }

        public override string ToString() => $"Position: ({this.X}, {this.Y})";

        public bool Equals(Position other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return obj is Position && Equals((Position) obj);
        }
        
        public static bool operator ==(Position a, Position b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Position a, Position b)
        {
            return !a.Equals(b);
        }

        public static Position operator +(Position a, Position b)
        {
            return new Position(a.X + b.X, a.Y + b.Y);
        }

        public static Position operator -(Position a, Position b)
        {
            return new Position(a.X - b.X, a.Y - b.Y);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (this.X * 397) ^ this.Y;
            }
        }
    }
}
