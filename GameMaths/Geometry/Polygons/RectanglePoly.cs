using System.Numerics;

namespace GameMaths.Geometry.Polygons
{
    public class RectanglePoly : Polygon
    {
        /// <summary>
        ///     Gets the direction of the Rectangle(Does not need update)
        /// </summary>
        public Vector2 Direction => (End - Start).Normalized();

        /// <summary>
        ///     Gets or sets the end.
        /// </summary>
        public Vector2 End { get; set; }

        /// <summary>
        ///     Gets a perpendicular direction of the Rectangle(Does not need an update)
        /// </summary>
        public Vector2 Perpendicular => Direction.Perpendicular();

        /// <summary>
        ///     Gets or sets the start.
        /// </summary>
        public Vector2 Start { get; set; }

        /// <summary>
        ///     Gets or sets the width.
        /// </summary>
        public float Width { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RectanglePoly" /> class.
        /// </summary>
        /// <param name="start">
        ///     The Start
        /// </param>
        /// <param name="end">
        ///     The End
        /// </param>
        /// <param name="width">
        ///     The Width
        /// </param>
        public RectanglePoly(Vector3 start, Vector3 end, float width) : this(start.ToVector2(), end.ToVector2(), width)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RectanglePoly" /> class.
        /// </summary>
        /// <param name="start">
        ///     The Start
        /// </param>
        /// <param name="end">
        ///     The End
        /// </param>
        /// <param name="width">
        ///     The Width
        /// </param>
        public RectanglePoly(Vector2 start, Vector2 end, float width)
        {
            Start = start;
            End = end;
            Width = width;
            UpdatePolygon();
        }

        /// <summary>
        ///     Updates the Polygon. Call this after changing something.
        /// </summary>
        /// <param name="offset">Extra width</param>
        /// <param name="overrideWidth">New width to use, overriding the set one.</param>
        public void UpdatePolygon(int offset = 0, float overrideWidth = -1)
        {
            Points.Clear();
            Points.Add(Start + ((overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular) - (offset * Direction));
            Points.Add(Start - ((overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular) - (offset * Direction));
            Points.Add(End - ((overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular) + (offset * Direction));
            Points.Add(End + ((overrideWidth > 0 ? overrideWidth : Width + offset) * Perpendicular) + (offset * Direction));
        }
    }
}