using GameServerCore.Packets.Enums;
using System.Numerics;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class AttentionPingRequest : ICoreRequest
    {
        public Vector2 Position { get; set; }
        public uint TargetNetID { get; set; }
        public Pings PingCategory { get; set; }

        public AttentionPingRequest(Vector2 position, uint targetNetId, Pings type)
        {
            Position = position;
            TargetNetID = targetNetId;
            PingCategory = type;
        }
    }
}
