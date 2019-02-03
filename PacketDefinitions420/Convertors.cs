using LeaguePackets.Game.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace PacketDefinitions420
{
    public static class Convertors
    {
        public static Vector2 WaypointToVector2(CompressedWaypoint cw)
        {
            return new Vector2(cw.X, cw.Y);
        }
    }
}
