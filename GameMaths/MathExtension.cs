using System;
using System.Numerics;

namespace GameMaths
{
    public static class MathExtension
    {
        public static Vector2 ToVector2(this Vector3 vector)
        {
            return new Vector2(vector.X, vector.Z);
        }
        public static float Distance(this Vector2 value1, Vector2 value2)
        {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;
            return (float)Math.Sqrt((x * x) + (y * y));
        }
        public static float DistanceSquared(this Vector2 value1, Vector2 value2)
        {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;
            return (x * x) + (y * y);
        }
        public static float Distance(this Vector3 value1, Vector3 value2)
        {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;
            float z = value1.Z - value2.Z;
            return (float)Math.Sqrt((x * x) + (y * y) + (z * z));
        }
        public static float DistanceSquared(this Vector3 value1, Vector3 value2)
        {
            float x = value1.X - value2.X;
            float y = value1.Y - value2.Y;
            float z = value1.Z - value2.Z;

            return (x * x) + (y * y) + (z * z);
        }
        public static float Dot(this Vector3 left, Vector3 right)
        {
            return (left.X * right.X) + (left.Y * right.Y) + (left.Z * right.Z);
        }
        public static Vector3 Rotated(this Vector3 vector3, float angle)
        {
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);

            return new Vector3(
                (float)((vector3.X * cos) - (vector3.Y * sin)),
                (float)((vector3.Y * cos) + (vector3.X * sin)),
                vector3.Z);
        }
        public static Vector2 Rotated(this Vector2 vector2, float angle)
        {
            var cos = Math.Cos(angle);
            var sin = Math.Sin(angle);

            return new Vector2(
                (float)((vector2.X * cos) - (vector2.Y * sin)),
                (float)((vector2.Y * cos) + (vector2.X * sin)));
        }
        public static bool IsZero(this float a)
        {
            return Math.Abs(a) < 1e-6f;
        }
        public static void Normalize(this Vector2 vector2)
        {
            float length = vector2.Length();
            if (!length.IsZero())
            {
                float inv = 1.0f / length;
                vector2.X *= inv;
                vector2.Y *= inv;
            }
        }
        public static Vector2 Normalized(this Vector2 vector2)
        {
            vector2.Normalize();
            return vector2;
        }
        public static Vector2 Perpendicular(this Vector2 vector2, int offset = 0)
        {
            return (offset == 0) ? new Vector2(-vector2.Y, vector2.X) : new Vector2(vector2.Y, -vector2.X);
        }
        public static Vector3 Perpendicular(this Vector3 vector3, int offset = 0)
        {
            return (offset == 0) ? new Vector3(-vector3.Y, vector3.X, vector3.Z) : new Vector3(vector3.Y, -vector3.X, vector3.Z);
        }
    }
}