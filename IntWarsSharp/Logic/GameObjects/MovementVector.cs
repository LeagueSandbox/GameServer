using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntWarsSharp.Logic.GameObjects
{
    public class MovementVector
    {
        const int MAP_WIDTH = 13982 / 2;
        const int MAP_HEIGHT = 14446 / 2;
        public short x = 0;
        public short y = 0;

        public MovementVector()
        {
        }

        public MovementVector(short x, short y)
        {
            this.x = x;
            this.y = y;
        }

        public MovementVector(float x, float y)
        {
            x = targetXToNormalFormat(x);
            y = targetYToNormalFormat(y);
        }

        public Target toTarget()
        {
            return new Target(2.0f * x + MAP_WIDTH, 2.0f * y + MAP_HEIGHT);
        }

        public static short targetXToNormalFormat(float _x)
        {
            return (short)(((_x) - MAP_WIDTH) / 2.0f);
        }

        public static short targetYToNormalFormat(float _y)
        {
            return (short)(((_y) - MAP_HEIGHT) / 2.0f);
        }

    }
}
