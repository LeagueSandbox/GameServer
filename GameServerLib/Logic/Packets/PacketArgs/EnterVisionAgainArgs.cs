using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs.DTO;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct EnterVisionAgainArgs
    {
        public UnitAtLocation Object { get; }
        public List<Vector2> Waypoints { get; }
        public int CurrentWaypoint { get; }

        public EnterVisionAgainArgs(UnitAtLocation o, List<Vector2> waypoints, int currentWaypoint)
        {
            Object = o;
            Waypoints = waypoints;
            CurrentWaypoint = currentWaypoint;
        }
    }
}
