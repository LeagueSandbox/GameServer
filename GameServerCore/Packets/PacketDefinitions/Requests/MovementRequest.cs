using GameServerCore.Enums;
using GameServerCore.Packets.Enums;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class MovementRequest : ICoreRequest
    {
        public uint NetId { get; }
        public OrderType Type { get; } //byte
        public Vector2 Position { get; }
        public uint TargetNetId { get; }
        public List<Vector2> Waypoints { get; }

        public MovementRequest(uint netId, uint targetNetId, Vector2 pos, OrderType order, List<Vector2> waypoints)
        {
            NetId = netId;
            TargetNetId = targetNetId;
            Position = pos;
            Type = order;
            Waypoints = waypoints;
        }
    }
}
