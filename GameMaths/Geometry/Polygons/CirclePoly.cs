using System;
using System.Numerics;

namespace GameMaths.Geometry.Polygons
{
    /// <summary>
    ///     Represents a Circle <see cref="Polygon" />
    /// </summary>
    public class CirclePoly : Polygon
    {
        /// <summary>
        ///     Gets or sets the Center of the Circle.
        /// </summary>
        public Vector2 Center { get; set; }

        /// <summary>
        ///     Gets or sets the Radius of Circle.
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        ///     Circle Quality
        /// </summary>
        private readonly int quality;


        /// <summary>
        ///     Initializes a new instance of the <see cref="CirclePoly" /> class.
        /// </summary>
        /// <param name="center">
        ///     The Center
        /// </param>
        /// <param name="radius">
        ///     The Radius
        /// </param>
        /// <param name="quality">
        ///     The Quality
        /// </param>
        public CirclePoly(Vector2 center, float radius, int quality = 20)
        {
            Center = center;
            Radius = radius;
            this.quality = quality;

            UpdatePolygon();
        }

        /// <summary>
        ///     Updates the Polygon. Call this after changing something.
        /// </summary>
        /// <param name="offset">Extra radius</param>
        /// <param name="overrideWidth">New width to use, overriding the set one.</param>
        public void UpdatePolygon(int offset = 0, float overrideWidth = -1)
        {
            Points.Clear();
            var outRadius = overrideWidth > 0 ? overrideWidth : (offset + Radius) / (float)Math.Cos(2 * Math.PI / quality);
            for (var i = 1; i <= quality; i++)
            {
                var angle = i * 2 * Math.PI / quality;
                var point = new Vector2(
                    Center.X + (outRadius * (float)Math.Cos(angle)),
                    Center.Y + (outRadius * (float)Math.Sin(angle)));
                Points.Add(point);
            }
        }
    }
}