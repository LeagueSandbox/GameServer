using System;
using System.Numerics;

namespace GameMaths.Geometry.Polygons
{
    public class SectorPoly : Polygon
    {
        /// <summary>
        ///     Gets or sets the angle.
        /// </summary>
        public float Angle { get; set; }

        /// <summary>
        ///     Gets or sets the center.
        /// </summary>
        public Vector2 Center { get; set; }

        /// <summary>
        ///     Gets or sets the direction.
        /// </summary>
        public Vector2 Direction { get; set; }

        /// <summary>
        ///     Gets or sets the radius.
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        ///     Local quality.
        /// </summary>
        private readonly int _quality;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SectorPoly" /> class.
        /// </summary>
        /// <param name="center">
        ///     The Center
        /// </param>
        /// <param name="direction">
        ///     The Direction
        /// </param>
        /// <param name="angle">
        ///     The Angle
        /// </param>
        /// <param name="radius">
        ///     The Radius
        /// </param>
        /// <param name="quality">
        ///     The Quality
        /// </param>
        public SectorPoly(Vector3 center, Vector3 direction, float angle, float radius, int quality = 20) : this(center.ToVector2(), direction.ToVector2(), angle, radius, quality)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="SectorPoly" /> class.
        /// </summary>
        /// <param name="center">
        ///     The Center
        /// </param>
        /// <param name="endPosition">
        ///     The end position
        /// </param>
        /// <param name="angle">
        ///     The Angle
        /// </param>
        /// <param name="radius">
        ///     The Radius
        /// </param>
        /// <param name="quality">
        ///     The Quality
        /// </param>
        public SectorPoly(Vector2 center, Vector2 endPosition, float angle, float radius, int quality = 20)
        {
            Center = center;
            Direction = (endPosition - center).Normalized();
            Angle = angle;
            Radius = radius;
            _quality = quality;
            UpdatePolygon();
        }

        /// <summary>
        ///     Updates the polygon. Call this after changing something.
        /// </summary>
        /// <param name="offset">Added radius</param>
        public void UpdatePolygon(int offset = 0)
        {
            Points.Clear();
            var outRadius = (Radius + offset) / (float)Math.Cos(2 * Math.PI / _quality);
            Points.Add(Center);
            var side1 = Direction.Rotated(-Angle * 0.5f);
            for (var i = 0; i <= _quality; i++)
            {
                var cDirection = side1.Rotated(i * Angle / _quality).Normalized();
                Points.Add(new Vector2(Center.X + (outRadius * cDirection.X), Center.Y + (outRadius * cDirection.Y)));
            }
        }
    }
}