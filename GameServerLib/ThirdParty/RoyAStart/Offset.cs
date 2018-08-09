using System;

namespace RoyT.AStar
{
    /// <summary>
    /// A 2D offset structure. You can use an array of offsets to represent the movement pattern
    /// of your agent, for example an offset of (-1, 0) means your character is able
    /// to move a single cell to the left <see cref="MovementPatterns"/> for some predefined
    /// options.
    /// </summary>
    public struct Offset : IEquatable<Offset>
    {
        private const float DiagonalCost = 1.4142135623730950488016887242097f; // sqrt(2)
        private const float LateralCost = 1.0f;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="x">x-movement offset</param>
        /// <param name="y">y-movement offset</param>
        public Offset(int x, int y)
        {
            if (x < -1 || x > 1)
                throw new ArgumentOutOfRangeException(nameof(x), $"Parameter {nameof(x)} cannot have a magnitude larger than one");

            if (y < -1 || y > 1)
                throw new ArgumentOutOfRangeException(nameof(y), $"Parameter {nameof(y)} cannot have a magnitude larger than one");

            if (x == 0 && y == 0)
                throw new ArgumentException(nameof(y), $"Paramters {nameof(x)} and {nameof(y)} cannot both be zero");

            this.X = x;
            this.Y = y;

            // Penalize diagonal movement
            this.Cost = (x != 0 && y != 0) ? DiagonalCost : LateralCost;                                   
        }

        /// <summary>
        /// X-position
        /// </summary>
        public int X { get; }

        /// <summary>
        /// Y-position
        /// </summary>
        public int Y { get; }

        /// <summary>
        /// Relative cost of adding this offset to a position, either 1 for lateral movement, or sqrt(2) for diagonal movement
        /// </summary>
        public float Cost { get; }

        public override string ToString() => $"Offset: ({this.X}, {this.Y})";
        
        public bool Equals(Offset other)
        {
            return this.X == other.X && this.Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;

            return obj is Offset && Equals((Offset)obj);
        }

        public static bool operator ==(Offset a, Offset b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(Offset a, Offset b)
        {
            return !a.Equals(b);
        }      

        public static Position operator +(Offset a, Position b)
        {
            return new Position(a.X + b.X, a.Y + b.Y);
        }

        public static Position operator -(Offset a, Position b)
        {
            return new Position(a.X - b.X, a.Y - b.Y);
        }

        public static Position operator +(Position a, Offset b)
        {
            return new Position(a.X + b.X, a.Y + b.Y);
        }

        public static Position operator -(Position a, Offset b)
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
