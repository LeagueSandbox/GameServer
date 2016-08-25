using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class MovementVector
    {
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

        public MovementVector(Unit u, float x, float y)
        {
            x = FormatCoordinate(x, u.GetGame().GetMap().GetHeight() / 2);
            y = FormatCoordinate(y, u.GetGame().GetMap().GetWidth() / 2);
        }

        public Target toTarget(Unit u)
        {
            
            return new Target(2.0f * x + (u.GetGame().GetMap().GetWidth() /2), 2.0f * y + (u.GetGame().GetMap().GetHeight() / 2));
        }

        public static short FormatCoordinate(float coordinate, float origin)
        {
            return (short)((coordinate - origin) / 2f);
        }

        public static short targetXToNormalFormat(Unit u, float value)
        {
            return FormatCoordinate(value, (u.GetGame().GetMap().GetWidth() / 2));
        }

        public static short targetYToNormalFormat(Unit u, float value)
        {
            return FormatCoordinate(value, (u.GetGame().GetMap().GetHeight() / 2));
        }
    }
}
