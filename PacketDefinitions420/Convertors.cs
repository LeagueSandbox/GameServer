using GameServerCore.Content;
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

        public static List<CompressedWaypoint> Vector2ToWaypoint(List<Vector2> wp, INavGrid grid)
        {
            return wp.ConvertAll(v => Vector2ToWaypoint(TranslateToCenteredCoordinates(v, grid)));
        }
        public static Vector2 WaypointToVector2(CompressedWaypoint cw)
        {
            return new Vector2(cw.X, cw.Y);
        }

        public static CompressedWaypoint Vector2ToWaypoint(Vector2 cw)
        {
            return new CompressedWaypoint((short)cw.X, (short)cw.Y);
        }

        public static Vector2 TranslateFromCenteredCoordinates(Vector2 vector, INavGrid grid)
        {
            // For unk reason coordinates are translated to 0,0 as a map center, so we gotta get back the original
            // mapSize contains the real center point coordinates, meaning width/2, height/2
            return new Vector2(2 * vector.X + grid.GetSize().X, 2 * vector.Y + grid.GetSize().Y);
        }

        public static Vector2 TranslateToCenteredCoordinates(Vector2 vector, INavGrid grid)
        {
            // For unk reason coordinates are translated to 0,0 as a map center, so we gotta get back the original
            // mapSize contains the real center point coordinates, meaning width/2, height/2
            return new Vector2((vector.X - grid.GetSize().X) / 2, (vector.Y - grid.GetSize().Y) / 2);
        }
    }
}
