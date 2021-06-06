using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using GameServerCore.Enums;
using GameServerCore.Packets.PacketDefinitions;

namespace GameServerCore
{
    /// <summary>
    /// Class housing miscellaneous functions usually meant to make calculations look cleaner.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Float constant used in small float comparison operations.
        /// </summary>
        public const float COMPARE_EPSILON = 0.0001f;

        /// <summary>
        /// Whether or not the given Vector2 is within the specified boundaries
        /// </summary>
        /// <param name="v">Vector2 to check.</param>
        /// <param name="max">Vector2 maximums to check against.</param>
        /// <param name="min">Vector2 minimums to check against.</param>
        /// <returns>True/False</returns>
        public static bool IsVectorValid(Vector2 v, Vector2 max, Vector2 min)
        {
            if (v == null)
            {
                return false;
            }

            return v.X <= max.X && v.Y <= max.Y && v.X >= min.X && v.Y >= min.Y;
        }

        /// <summary>
        /// Gets the squared length of the specified Vector2.
        /// </summary>
        /// <param name="v">Vector2 who's length should be squared.</param>
        /// <returns>Squared length of v</returns>
        public static float SqrLength(this Vector2 v)
        {
            return v.X * v.X + v.Y * v.Y;
        }

        /// <summary>
        /// Gets the number of nanoseconds that have passed since the given StopWatch was started.
        /// Unused (could probably be removed honestly).
        /// </summary>
        /// <param name="watch">StopWatch to check.</param>
        /// <returns>Nanoseconds elapsed since watch started</returns>
        public static long ElapsedNanoSeconds(this Stopwatch watch)
        {
            return watch.ElapsedTicks * 1000000000 / Stopwatch.Frequency;
        }

        /// <summary>
        /// Gets the number of microseconds that have passed since the given StopWatch was started.
        /// Unused (could probably be removed honestly).
        /// </summary>
        /// <param name="watch">StopWatch to check.</param>
        /// <returns>Microseconds elapsed since the watch started.</returns>
        public static long ElapsedMicroSeconds(this Stopwatch watch)
        {
            return watch.ElapsedTicks * 1000000 / Stopwatch.Frequency;
        }

        /// <summary>
        /// Performs addition between an integer value and a list of bytes.
        /// Converts the integer into an array of bytes then adds the array to the list.
        /// </summary>
        /// <param name="list">List of bytes to add to.</param>
        /// <param name="val">Integer to add to the list of bytes.</param>
        public static void Add(this List<byte> list, int val)
        {
            list.AddRange(PacketHelper.IntToByteArray(val));
        }

        /// <summary>
        /// Performs addition between a string and a list of bytes.
        /// Converts the string into an array of bytes, then adds the array to the list of bytes.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="val"></param>
        public static void Add(this List<byte> list, string val)
        {
            list.AddRange(Encoding.BigEndianUnicode.GetBytes(val));
        }

        /// <summary>
        /// Rotates v clockwise by the given angle with respect to the origin.
        /// </summary>
        /// <param name="v">Vector2 to rotate.</param>
        /// <param name="origin">Vector2 point to rotate around.</param>
        /// <param name="angle">Degrees to rotate by.</param>
        /// <returns>Rotated Vector2</returns>
        public static Vector2 Rotate(this Vector2 v, Vector2 origin, float angle)
        {
            // Rotating (px,py) around (ox, oy) with angle a
            // p'x = cos(a) * (px-ox) - sin(a) * (py-oy) + ox
            // p'y = sin(a) * (px-ox) + cos(a) * (py-oy) + oy
            angle = -DegreeToRadian(angle);
            var x = MathF.Cos(angle) * (v.X - origin.X) - MathF.Sin(angle) * (v.Y - origin.Y) + origin.X;
            var y = MathF.Sin(angle) * (v.X - origin.X) + MathF.Cos(angle) * (v.Y - origin.Y) + origin.Y;
            return new Vector2(x, y);
        }

        /// <summary>
        /// Rotates a Vector2 about the standard origin (0,0) by the given angle in degrees.
        /// </summary>
        /// <param name="v">Vector2 to rotate.</param>
        /// <param name="angle">Degrees to rotate.</param>
        /// <returns>Rotated Vector2</returns>
        public static Vector2 Rotate(this Vector2 v, float angle)
        {
            return v.Rotate(new Vector2(0, 0), angle);
        }

        /// <summary>
        /// Gets the angle from one Vector2 point to another relative to an origin point.
        /// </summary>
        /// <param name="v">Vector2 to start from.</param>
        /// <param name="vectorToGetAngle">Vector2 to point towards.</param>
        /// <param name="origin">Vector2 to orient around.</param>
        /// <returns>float Angle in degrees</returns>
        public static float AngleTo(this Vector2 v, Vector2 vectorToGetAngle, Vector2 origin)
        {
            // Make other vectors relative to the origin
            v.X -= origin.X;
            vectorToGetAngle.X -= origin.X;
            v.Y -= origin.Y;
            vectorToGetAngle.Y -= origin.Y;

            var norm = Vector2.Normalize(vectorToGetAngle - v);

            return UnitVectorToAngle(norm);
        }

        /// <summary>
        /// Forces the float value to remain within the specified min/max values.
        /// </summary>
        /// <param name="value">Float to clamp.</param>
        /// <param name="minValue">Minimum value.</param>
        /// <param name="maxValue">Maximum value.</param>
        /// <returns>Clamped float.</returns>
        public static float Clamp(this float value, float minValue, float maxValue)
        {
            return value < minValue ? minValue : MathF.Min(value, maxValue);
        }

        /// <summary>
        /// Calculates given triangle's area using Heron's formula.
        /// </summary>
        /// <param name="first">First corner of the triangle.</param>
        /// <param name="second">Second corner of the triangle</param>
        /// <param name="third">Third corner of the triangle.</param>
        /// <returns>the area of the triangle.</returns>
        public static float GetTriangleArea(Vector2 first, Vector2 second, Vector2 third)
        {
            var line1Length = Vector2.Distance(first, second);
            var line2Length = Vector2.Distance(second, third);
            var line3Length = Vector2.Distance(third, first);

            var s = (line1Length + line2Length + line3Length) / 2;

            return (float)Math.Sqrt(s * (s - line1Length) * (s - line2Length) * (s - line3Length));
        }

        /// <summary>
        /// Gets the squared distance from a specific point to a rectangle's border.
        /// </summary>
        /// <param name="rect">Rectangle center point.</param>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        /// <param name="origin">Point to check distance from.</param>
        /// <param name="rotation">Optional rotation of the rectangle in degrees (expensive).</param>
        /// <returns>Float squared distance.</returns>
        public static float DistanceSquaredToRectangle(Vector2 rect, float width, float height, Vector2 origin, float rotation = 0)
        {
            if (rotation != 0)
            {
                rect = Rotate(rect, rect, rotation);
                origin = Rotate(origin, rect, rotation);
            }

            float dx = MathF.Max(MathF.Abs(origin.X - rect.X) - width / 2, 0);
            float dy = MathF.Max(MathF.Abs(origin.Y - rect.Y) - height / 2, 0);

            return dx * dy + dy * dy;
        }

        /// <summary>
        /// Whether or not the given origin point lies inside the boundaries of the given rectangle.
        /// </summary>
        /// <param name="rect">Center point of the rectangle.</param>
        /// <param name="width">Width of the rectangle.</param>
        /// <param name="height">Height of the rectangle.</param>
        /// <param name="origin">Origin point to check.</param>
        /// <param name="rotation">Optional rotation of the rectangle in degrees (expensive).</param>
        /// <returns></returns>
        public static bool IsPointInRectangle(Vector2 rect, float width, float height, Vector2 origin, float rotation = 0)
        {
            if (rotation != 0)
            {
                // What? You thought the rectangle rotated? No, everything rotated around the rectangle.
                rect = Rotate(rect, rect, rotation);
                origin = Rotate(origin, rect, rotation);
            }

            float dx = MathF.Max(MathF.Abs(origin.X - rect.X) - width / 2, 0);
            float dy = MathF.Max(MathF.Abs(origin.Y - rect.Y) - height / 2, 0);

            if (dx == 0 && dy == 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Whether or not the specified Vector2 v is within the specified range of the given Vector2 start.
        /// </summary>
        /// <param name="v">Vector2 to check.</param>
        /// <param name="start">Vector2 where the range starts.</param>
        /// <param name="range">Range to check around the start position.</param>
        /// <returns></returns>
        public static bool IsVectorWithinRange(Vector2 v, Vector2 start, float range)
        {
            float v1 = v.X - start.X, v2 = v.Y - start.Y;
            return ((v1 * v1) + (v2 * v2)) <= (range * range);
        }

        /// <summary>
        /// Gets the angle from one Vector2 to another.
        /// Origin is (0,0).
        /// </summary>
        /// <param name="v">Starting point.</param>
        /// <param name="vectorToGetAngle">Ending point.</param>
        /// <returns>Angle in degrees.</returns>
        public static float AngleTo(this Vector2 v, Vector2 vectorToGetAngle)
        {
            return v.AngleTo(vectorToGetAngle, new Vector2(0, 0));
        }

        /// <summary>
        /// Converts the given angle from degrees to radians.
        /// </summary>
        /// <param name="angle">Angle in degrees.</param>
        /// <returns>Angle in radians.</returns>
        public static float DegreeToRadian(float angle)
        {
            return MathF.PI * angle / 180.0f;
        }

        /// <summary>
        /// Converts the given angle from radians to degrees.
        /// </summary>
        /// <param name="angle">Angle in radians.</param>
        /// <returns>Angle in degrees.</returns>
        public static float RadianToDegree(float angle)
        {
            return angle * (180.0f / MathF.PI);
        }

        /// <summary>
        /// Converts the given normalized vector (such that |v| = 1) to an angle in degrees (0 -> 360).
        /// </summary>
        /// <param name="v">Vector2 to convert.</param>
        /// <returns>Angle in degrees (0 -> 360)</returns>
        public static float UnitVectorToAngle(Vector2 v)
        {
            var angle = RadianToDegree(MathF.Atan2(v.Y, v.X));

            // Clamp Atan2 degrees to 0 -> 360.
            return (angle + 360f) % 360f;
        }

        /// <summary>
        /// Gets the closest edge point on a circle from the given starting position.
        /// </summary>
        /// <param name="from">Starting position.</param>
        /// <param name="circlePos">Circle position.</param>
        /// <param name="radius">Radius of the circle.</param>
        /// <returns>Vector2 point on the circle closest to the starting position..</returns>
        public static Vector2 GetClosestCircleEdgePoint(Vector2 from, Vector2 circlePos, float radius)
        {
            return new Vector2(circlePos.X + (MathF.Cos(MathF.Atan2(from.Y - circlePos.Y, from.X - circlePos.X)) * radius),
                               circlePos.Y + (MathF.Sin(MathF.Atan2(from.Y - circlePos.Y, from.X - circlePos.X)) * radius));
        }

        /// <summary>
        /// Attempts to find the points of intersection between a line and a circle which is located at (0,0)
        /// </summary>
        /// <param name="p1">Closest line endpoint to the circle</param>
        /// <param name="p2">Second closest line endpoint to the circle</param>
        /// <param name="r">Radius of the circle</param>
        /// <returns>0 to 2 points of intersection</returns>
        public static List<Vector2> CircleLineIntersection(Vector2 p1, Vector2 p2, float r)
        {
            List<Vector2> intersections = new List<Vector2>();

            float dx = p2.X - p1.X;
            float dy = p2.Y - p1.Y;
            float dr = MathF.Sqrt((dx * dx) + (dy * dy));
            // Dot Product
            float D = (p1.X * p2.Y) - (p2.X * p1.Y);

            // Sign variable to account for the square rooting of distance;
            // this leads into two intersections
            int sgn = 1;
            if (dy < 0)
            {
                sgn = -1;
            }

            // Make nullable floats so we can check for 0 intersections
            float? x1 = (D * dy + (sgn * dx * MathF.Sqrt((r * r) * (dr * dr) - (D * D)))) / (dr * dr);
            float? y1 = (-D * dx + (MathF.Abs(dy) * MathF.Sqrt((r * r) * (dr * dr) - (D * D)))) / (dr * dr);
            float? x2 = (D * dy - (sgn * dx * MathF.Sqrt((r * r) * (dr * dr) - (D * D)))) / (dr * dr);
            float? y2 = (-D * dx - (MathF.Abs(dy) * MathF.Sqrt((r * r) * (dr * dr) - (D * D)))) / (dr * dr);

            float discriminant = (r * r) * (dr * dr) - (D * D);

            // Checking for no intersections
            if (discriminant > 0 && (x1 != null || x1.GetValueOrDefault() != 0) && (x2 != null || x2.GetValueOrDefault() != 0))
            {
                intersections.Add(new Vector2((float)x1, (float)y1));
                intersections.Add(new Vector2((float)x2, (float)y2));
            }
            // In the case of 1 intersection
            else if (discriminant == 0)
            {
                // Check which side intersected
                if (x1 != null || x1.GetValueOrDefault() != 0)
                {
                    intersections.Add(new Vector2((float)x1, (float)y1));
                }
                else if (x2 != null || x2.GetValueOrDefault() != 0)
                {
                    intersections.Add(new Vector2((float)x2, (float)y2));
                }
            }

            return intersections;
        }

        /// <summary>
        /// Attempts to find an escape point for the first given circle.
        /// Should only be used when the two given circles are intersecting.
        /// </summary>
        /// <param name="p1">Position of the first circle</param>
        /// <param name="r1">Radius of the first circle</param>
        /// <param name="p2">Position of the second circle</param>
        /// <param name="r2">Radius of the second circle</param>
        /// <returns></returns>
        public static Vector2 GetCircleEscapePoint(Vector2 p1, float r1, Vector2 p2, float r2)
        {
            Vector2 edgepoint1 = GetClosestCircleEdgePoint(p2, p1, r1);
            Vector2 edgepoint2 = GetClosestCircleEdgePoint(p1, p2, r2);

            return Vector2.Add(p1, Vector2.Subtract(edgepoint2, edgepoint1));
        }

        /// <summary>
        /// Casts two rays from point p to the left and right boundaries of a given circle.
        /// </summary>
        /// <param name="p">Point to cast the rays from.</param>
        /// <param name="c">Center of the circle.</param>
        /// <param name="r">Radius of the circle.</param>
        /// <returns>Array of 2 points representing the left and right bounds of the circle respectively.</returns>
        /// TODO: Could probably be more efficient by using an alternative to Cos and Sin.
        public static Vector2[] CastRayCircleBounds(Vector2 p, Vector2 c, float r)
        {
            var angleToCenter = p.AngleTo(c, p);
            var angleToLeft = angleToCenter + 270f;
            var angleToRight = angleToCenter + 90f;

            var cLeftBound = new Vector2(c.X + (MathF.Cos(angleToLeft) * r), c.Y + (MathF.Sin(angleToLeft) * r));
            var cRightBound = new Vector2(c.X + (MathF.Cos(angleToRight) * r), c.Y + (MathF.Sin(angleToRight) * r));

            return new Vector2[] { cLeftBound, cRightBound };
        }
    }

    /// <summary>
    /// Class for simple conversions between mathematical values and enums.
    /// </summary>
    public static class CustomConvert
    {
        /// <summary>
        /// Converts an integer to the TeamId assigned to it.
        /// </summary>
        /// <param name="i">Integer to convert.</param>
        /// <returns>TeamId.</returns>
        public static TeamId ToTeamId(this int i)
        {
            var dic = new Dictionary<int, TeamId>
            {
                { 0, TeamId.TEAM_BLUE },
                { (int)TeamId.TEAM_BLUE, TeamId.TEAM_BLUE },
                { 1, TeamId.TEAM_PURPLE },
                { (int)TeamId.TEAM_PURPLE, TeamId.TEAM_PURPLE }
            };

            if (!dic.ContainsKey(i))
            {
                return (TeamId)2;
            }

            return dic[i];
        }

        /// <summary>
        /// Converts the TeamId to the integer representing it.
        /// </summary>
        /// <param name="team">TeamId to convert.</param>
        /// <returns>Integer.</returns>
        public static int FromTeamId(this TeamId team)
        {
            var dic = new Dictionary<TeamId, int>
            {
                { TeamId.TEAM_BLUE, 0 },
                { TeamId.TEAM_PURPLE, 1 }
            };

            if (!dic.ContainsKey(team))
            {
                return 2;
            }

            return dic[team];
        }

        /// <summary>
        /// Gets the opposite team of the given team.
        /// </summary>
        /// <param name="team">TeamId to check.</param>
        /// <returns>Enemy TeamId.</returns>
        public static TeamId GetEnemyTeam(this TeamId team)
        {
            var dic = new Dictionary<TeamId, TeamId>
            {
                { TeamId.TEAM_BLUE, TeamId.TEAM_PURPLE },
                { TeamId.TEAM_PURPLE, TeamId.TEAM_BLUE }
            };

            if (!dic.ContainsKey(team))
            {
                return (TeamId)2;
            }

            return dic[team];
        }
    }
}
