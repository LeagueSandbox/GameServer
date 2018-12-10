using GameServerCore.Packets.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class EmotionPacketRequest : ICoreRequest
    {
        public uint NetId;
        public Emotions Id;

        public EmotionPacketRequest(uint netId, Emotions id)
        {
            NetId = netId;
            Id = id;
        }
    }
}
