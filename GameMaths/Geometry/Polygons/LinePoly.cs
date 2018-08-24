using System.Numerics;

namespace GameMaths.Geometry.Polygons
{
    public class LinePoly : Polygon
    {
        /// <summary>
        ///     Gets or sets the End of the Line.
        /// </summary>
        public Vector2 LineEnd { get; set; }

        /// <summary>
        ///     Gets or sets the Start of the Line.
        /// </summary>
        public Vector2 LineStart { get; set; }

        /// <summary>
        ///     Gets or sets the length of the Line. (Does not have to be updated)
        /// </summary>
        public float Length
        {
            get
            {
                return LineStart.Distance(LineEnd);
            }

            set
            {
                LineEnd = ((LineEnd - LineStart).Normalized() * value) + LineStart;
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LinePoly" /> class.
        /// </summary>
        /// <param name="start">
        ///     The Start
        /// </param>
        /// <param name="end">
        ///     The End
        /// </param>
        /// <param name="length">
        ///     Length of line(will be automatically set if -1)
        /// </param>
        public LinePoly(Vector3 start, Vector3 end, float length = -1) : this(start.ToVector2(), end.ToVector2(), length)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="LinePoly" /> class.
        /// </summary>
        /// <param name="start">
        ///     The Start
        /// </param>
        /// <param name="end">
        ///     The End
        /// </param>
        /// <param name="length">
        ///     Length of line(will be automatically set if -1)
        /// </param>
        public LinePoly(Vector2 start, Vector2 end, float length = -1)
        {
            LineStart = start;
            LineEnd = end;
            if (length > 0)
            {
                Length = length;
            }
            UpdatePolygon();
        }

        /// <summary>
        ///     Updates the polygon. Use this after changing something.
        /// </summary>
        public void UpdatePolygon()
        {
            Points.Clear();
            Points.Add(LineStart);
            Points.Add(LineEnd);
        }
    }
}