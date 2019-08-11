using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace GameMaths.Geometry.Polygons
{
    /// <summary>
    ///     Base class representing a polygon.
    /// </summary>
    public class Polygon
    {
        /// <summary>
        ///     Gets or sets the list of all points in the polygon.
        /// </summary>
        public List<Vector2> Points { get; set; } = new List<Vector2>();

        /// <summary>
        ///     Converts Vector3 to 2D, then adds it to the points.
        /// </summary>
        /// <param name="point">The Point</param>
        public void Add(Vector2 point)
        {
            Points.Add(point);
        }

        /// <summary>
        ///     Adds all of the points in the polygon to this instance.
        /// </summary>
        /// <param name="polygon">The Polygon</param>
        public void Add(Polygon polygon)
        {
            foreach (var point in polygon.Points)
            {
                Points.Add(point);
            }
        }

        /// <summary>
        ///     Gets if the Vector3 is inside the polygon.
        /// </summary>
        /// <param name="point">The Point</param>
        /// <returns>Whether the Vector3 is inside the polygon</returns>
        public bool IsInside(Vector2 point)
        {
            return !IsOutside(point);
        }

        /// <summary>
        ///     Gets if the position is outside of the polygon.
        /// </summary>
        /// <param name="point">The Point</param>
        /// <returns>Whether the Vector2 is inside the polygon</returns>
        public bool IsOutside(Vector2 point)
        {
            var p = new IntPoint((long)point.X, (long)point.Y);
            return Clipper.PointInPolygon(p, ToClipperPath()) != 1;
        }

        public bool CheckForOverLaps(Polygon otherPolygon)
        {
            var subj = new List<List<IntPoint>>();
            var clip = new List<List<IntPoint>>();

            subj.Add(ToClipperPath());
            clip.Add(otherPolygon.ToClipperPath());

            var solution = new List<List<IntPoint>>();
            var c = new Clipper();

            c.AddPaths(subj, PolyType.ptSubject, true);
            c.AddPaths(clip, PolyType.ptClip, true);

            c.Execute(ClipType.ctIntersection, solution, PolyFillType.pftNonZero, PolyFillType.pftNonZero);
            return solution.Count != 0;
        }

        /// <summary>
        ///     Converts all the points to the Clipper Library format
        /// </summary>
        /// <returns>List of <c>IntPoint</c>'s</returns>
        public List<IntPoint> ToClipperPath()
        {
            var result = new List<IntPoint>(this.Points.Count);
            result.AddRange(Points.Select(point => new IntPoint((long)point.X, (long)point.Y)));
            return result;
        }
    }
}