using System;
using System.Numerics;

namespace GameMaths.Geometry.Polygons
{
    public class ArcPoly : Polygon
    {
        /// <summary>
        ///     Gets or sets the angle.
        /// </summary>
        public float Angle { get; set; }

        /// <summary>
        ///     Gets or sets the end position.
        /// </summary>
        public Vector2 EndPos { get; set; }

        /// <summary>
        ///     Gets or sets the radius.
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        ///     Gets or sets the start position.
        /// </summary>
        public Vector2 StartPos { get; set; }
        /// <summary>
        ///     Arc Quality
        /// </summary>
        private readonly int _quality;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ArcPoly" /> class, after converting the points to 2D.
        /// </summary>
        /// <param name="start">
        ///     Start of the Arc
        /// </param>
        /// <param name="direction">
        ///     Direction of the Arc
        /// </param>
        /// <param name="angle">
        ///     Angle of the Arc
        /// </param>
        /// <param name="radius">
        ///     Radius of the Arc
        /// </param>
        /// <param name="quality">
        ///     Quality of the Arc
        /// </param>
        public ArcPoly(Vector3 start, Vector3 direction, float angle, float radius, int quality = 20) : this(start.ToVector2(), direction.ToVector2(), angle, radius, quality)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ArcPoly" /> class.
        /// </summary>
        /// <param name="start">
        ///     Start of the Arc
        /// </param>
        /// <param name="end">
        ///     End of the Arc
        /// </param>
        /// <param name="angle">
        ///     Angle of the Arc
        /// </param>
        /// <param name="radius">
        ///     Radius of the Arc
        /// </param>
        /// <param name="quality">
        ///     Quality of the Arc
        /// </param>
        public ArcPoly(Vector2 start, Vector2 end, float angle, float radius, int quality = 20)
        {
            StartPos = start;
            EndPos = (end - start).Normalized();
            Angle = angle;
            Radius = radius;
            _quality = quality;
            UpdatePolygon();
        }
        /// <summary>
        ///     Updates the Arc. Use this after changing something.
        /// </summary>
        /// <param name="offset">Radius extra offset</param>
        public void UpdatePolygon(int offset = 0)
        {
            Points.Clear();
            var outRadius = (Radius + offset) / (float)Math.Cos(2 * Math.PI / _quality);
            var side1 = EndPos.Rotated(-Angle * 0.5f);
            for (var i = 0; i <= _quality; i++)
            {
                var cDirection = side1.Rotated(i * Angle / _quality).Normalized();
                Points.Add(
                    new Vector2(
                        StartPos.X + (outRadius * cDirection.X),
                        StartPos.Y + (outRadius * cDirection.Y)));
            }
        }
    }
}