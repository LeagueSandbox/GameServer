using GameServerCore.Enums;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class MovementRequest : ICoreRequest
    {
        public OrderType OrderType { get; }
        public Vector2 Position { get; }
        public uint TargetNetID { get; }
        public uint TeleportNetID { get;  }
        public bool HasTeleportID { get;  }
        public byte TeleportID { get;  }
        public List<Vector2> Waypoints { get; }

        public MovementRequest(OrderType orderType, Vector2 position, uint targetNetId, uint teleportNetId, bool hasTeleportId, List<Vector2> waypoints)
        {
            OrderType = orderType;
            Position = position;
            TargetNetID = targetNetId;
            TeleportNetID = teleportNetId;
            HasTeleportID = hasTeleportId;
            Waypoints = waypoints;
        }
    }
}
