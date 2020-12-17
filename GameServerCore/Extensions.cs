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
            angle = (float)-DegreeToRadian(angle);
            var x = (float)(Math.Cos(angle) * (v.X - origin.X) - Math.Sin(angle) * (v.Y - origin.Y) + origin.X);
            var y = (float)(Math.Sin(angle) * (v.X - origin.X) + Math.Cos(angle) * (v.Y - origin.Y) + origin.Y);
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
        public static float AngleBetween(this Vector2 v, Vector2 vectorToGetAngle, Vector2 origin)
        {
            // Make other vectors relative to the origin
            v.X -= origin.X;
            vectorToGetAngle.X -= origin.X;
            v.Y -= origin.Y;
            vectorToGetAngle.Y -= origin.Y;

            // Normalize the vectors
            v = Vector2.Normalize(v);
            vectorToGetAngle = Vector2.Normalize(vectorToGetAngle);

            // Get the angle
            var ang = Vector2.Dot(v, vectorToGetAngle);
            var returnVal = (float)RadianToDegree(Math.Acos(ang));
            if (vectorToGetAngle.X < v.X)
            {
                returnVal = 360 - returnVal;
            }

            return returnVal;
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
            return value < minValue ? minValue : Math.Min(value, maxValue);
        }

        /// <summary>
        /// Gets the angle from one Vector2 to another.
        /// Origin is (0,0).
        /// </summary>
        /// <param name="v">Starting point.</param>
        /// <param name="vectorToGetAngle">Ending point.</param>
        /// <returns>Angle in degrees.</returns>
        public static float AngleBetween(this Vector2 v, Vector2 vectorToGetAngle)
        {
            return v.AngleBetween(vectorToGetAngle, new Vector2(0, 0));
        }

        /// <summary>
        /// Converts the given angle from degrees to radians.
        /// </summary>
        /// <param name="angle">Angle in degrees.</param>
        /// <returns>Angle in radians.</returns>
        public static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        /// <summary>
        /// Converts the given angle from radians to degrees.
        /// </summary>
        /// <param name="angle">Angle in radians.</param>
        /// <returns>Angle in degrees.</returns>
        public static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
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
            return new Vector2(circlePos.X + (float)(Math.Cos(Math.Atan2(from.Y - circlePos.Y, from.X - circlePos.X)) * Math.Sqrt(radius)),
                               circlePos.Y + (float)(Math.Sin(Math.Atan2(from.Y - circlePos.Y, from.X - circlePos.X)) * Math.Sqrt(radius)));
        }

        /// <summary>
        /// Gets the area of the circular segment formed by the intersection of a line formed by the two given points with a circle.
        /// </summary>
        /// <param name="p1">Starting position of a line.</param>
        /// <param name="p2">Ending position of a line.</param>
        /// <param name="origin">Position of the circle.</param>
        /// <param name="r">Radius of the circle.</param>
        /// <returns>Area of the circular segment.</returns>
        public static double CircularSegmentArea(Vector2 p1, Vector2 p2, Vector2 origin, float r)
        {
            float l = Math.Abs(Vector2.Distance(p1, p2));
            double angle = DegreeToRadian(AngleBetween(p1, p1, origin));
            double h = r * Math.Cos((1 / 2) * angle);
            double arcl = r * angle;

            double sectorArea = (1 / 2) * r * arcl;
            double triArea = (1 / 2) * l * h;

            return sectorArea - triArea;
        }

        /// <summary>
        /// Finds the points of intersection between a line and a circle which is located at (0,0)
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
            double dr = Math.Sqrt((dx * dx) + (dy * dy));
            float D = (p1.X * p2.Y) - (p2.X * p1.Y);

            int sgn = 1;
            if (dy < 0)
            {
                sgn = -1;
            }

            double? x1 = (D * dy + (sgn * dx * Math.Sqrt((r * r) * (dr * dr) - (D * D)))) / (dr * dr);
            double? y1 = (-D * dx + (Math.Abs(dy) * Math.Sqrt((r * r) * (dr * dr) - (D * D)))) / (dr * dr);
            double? x2 = (D * dy - (sgn * dx * Math.Sqrt((r * r) * (dr * dr) - (D * D)))) / (dr * dr);
            double? y2 = (-D * dx - (Math.Abs(dy) * Math.Sqrt((r * r) * (dr * dr) - (D * D)))) / (dr * dr);

            double discriminant = (r * r) * (dr * dr) - (D * D);

            if (discriminant > 0 && (x1 != null || x1.GetValueOrDefault() != 0) && (x2 != null || x2.GetValueOrDefault() != 0))
            {
                intersections.Add(new Vector2((float)x1, (float)y1));
                intersections.Add(new Vector2((float)x2, (float)y2));
            }
            else if (discriminant == 0)
            {
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
