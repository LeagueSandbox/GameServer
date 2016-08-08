using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
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
            x = FormatCoordinate(x, MAP_WIDTH);
            y = FormatCoordinate(y, MAP_HEIGHT);
        }

        public Target toTarget()
        {
            return new Target(2.0f * x + MAP_WIDTH, 2.0f * y + MAP_HEIGHT);
        }

        public static short FormatCoordinate(float coordinate, float origin)
        {
            return (short)((coordinate - origin) / 2f);
        }

        public static short targetXToNormalFormat(float value)
        {
            return FormatCoordinate(value, MAP_WIDTH);
        }

        public static short targetYToNormalFormat(float value)
        {
            return FormatCoordinate(value, MAP_HEIGHT);
        }
    }
}
