using GameServerCore.Packets.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class AttentionPingRequest : ICoreRequest
    {
        public float X { get; }
        public float Y { get; }
        public int TargetNetId { get; }
        public Pings Type { get; }

        public AttentionPingRequest(float x, float y, int targetNetId, Pings type)
        {
            X = x;
            Y = y;
            TargetNetId = targetNetId;
            Type = type;
        }
    }
}
