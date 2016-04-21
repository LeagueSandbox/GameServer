using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic
{
    /// <summary>
    /// Math extensions
    /// </summary>
    public static class MathExt
    {
        public const float FP_TOLERANCE = 10e-10f;

        public static float Vec2Cross(Vector2 first, Vector2 second)
        {
            return first.X * second.Y - second.X * first.Y;
        }

        public static bool FloatEqual(float a, float b, float tolerance = FP_TOLERANCE)
        {
            return (Math.Abs(a - b) <= tolerance);
        }

        public static bool VectorEqual(Vector2 a, Vector2 b)
        {
            return FloatEqual(a.X, b.X) && FloatEqual(a.Y, b.Y);
        }
    }
}
