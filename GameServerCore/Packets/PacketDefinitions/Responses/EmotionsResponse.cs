using GameServerCore.Packets.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class EmotionsResponse : ICoreResponse
    {
        public Emotions Type { get; }
        public uint NetId { get; }
        public EmotionsResponse(Emotions type, uint netId)
        {
            Type = type;
            NetId = netId;
        }
    }
}