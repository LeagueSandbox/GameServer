using System;
using System.Numerics;

namespace GameMaths.Geometry.Polygons
{
    public class RingPoly : Polygon
    {
        /// <summary>
        ///     Gets or sets the center.
        /// </summary>
        public Vector2 Center { get; set; }

        /// <summary>
        ///     Gets or sets the outer radius.
        /// </summary>
        public float OuterRadius { get; set; }

        /// <summary>
        ///     Gets or sets the ring width
        /// </summary>
        public float Width { get; set; }
        /// <summary>
        ///     Ring Quality
        /// </summary>
        private readonly int _quality;

        /// <summary>
        ///     Initializes a new instance of the <see cref="RingPoly" /> class.
        /// </summary>
        /// <param name="center">
        ///     The Center
        /// </param>
        /// <param name="width">
        ///     The ring width
        /// </param>
        /// <param name="outerRadius">
        ///     Outer Radius
        /// </param>
        /// <param name="quality">
        ///     The Quality
        /// </param>
        public RingPoly(Vector3 center, float width, float outerRadius, int quality = 20) : this(center.ToVector2(), width, outerRadius, quality)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="RingPoly" /> class.
        /// </summary>
        /// <param name="center">
        ///     The Center
        /// </param>
        /// <param name="width">
        ///     The ring width
        /// </param>
        /// <param name="outerRadius">
        ///     Outer Radius
        /// </param>
        /// <param name="quality">
        ///     The Quality
        /// </param>
        public RingPoly(Vector2 center, float width, float outerRadius, int quality = 20)
        {
            Center = center;
            Width = width;
            OuterRadius = outerRadius;
            _quality = quality;
            UpdatePolygon();
        }
        /// <summary>
        ///     Updates the polygon. Call this after you change something.
        /// </summary>
        /// <param name="offset">Added radius</param>
        public void UpdatePolygon(int offset = 0)
        {
            Points.Clear();
            var outRadius = (offset + Width + OuterRadius) / (float)Math.Cos(2 * Math.PI / _quality);
            var innerRadius = Width - OuterRadius - offset;
            for (var i = 0; i <= _quality; i++)
            {
                var angle = i * 2 * Math.PI / _quality;

                var point = new Vector2(Center.X - (outRadius * (float)Math.Cos(angle)), Center.Y - (outRadius * (float)Math.Sin(angle)));
                Points.Add(point);

                var point2 = new Vector2(Center.X + (innerRadius * (float)Math.Cos(angle)), Center.Y - (innerRadius * (float)Math.Sin(angle)));
                Points.Add(point2);
            }
        }
    }
}