using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic
{
    public static class Helpers
    {
        private const double DegToRad = Math.PI / 180;

        public static Vector2 Rotate(this Vector2 v, float degrees)
        {
            return RotateRadians(v, degrees * ((float)DegToRad));
        }

        public static Vector2 RotateRadians(this Vector2 v, float radians)
        {
            var ca = (float)Math.Cos(radians);
            var sa = (float)Math.Sin(radians);
            return new Vector2((float)(ca * v.X - sa * v.Y), ((float)sa * v.X + ca * v.Y));
        }
    }
}
