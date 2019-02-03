using GameServerCore.Packets.Enums;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class MovementRequest : ICoreRequest
    {
        public uint NetId { get; }
        public MoveType Type { get; } //byte
        public Vector2 Position{ get; }
        public uint TargetNetId { get; }
        public List<Vector2> Waypoints { get; }

        public MovementRequest(uint netId, uint targetNetId, Vector2 pos, MoveType order,List<Vector2> waypoints)
        {
            NetId = netId;
            Type = order;
            Position = pos;
            targetNetId = TargetNetId;
            Waypoints = waypoints;
        }
    }
}
