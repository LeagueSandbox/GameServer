using GameServerCore.Packets.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class EmotionPacketRequest : ICoreRequest
    {
        public Emotions EmoteID;

        public EmotionPacketRequest(Emotions emoteId)
        {
            EmoteID = emoteId;
        }
    }
}
