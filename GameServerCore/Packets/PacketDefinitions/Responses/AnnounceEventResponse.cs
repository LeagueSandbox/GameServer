using GameServerCore.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class AnnounceEventResponse : ICoreResponse
    {
        public int MapId { get; }
        public Announces MessageId { get; }
        public bool IsMapSpecific { get; }
        public AnnounceEventResponse(int mapId, Announces messageId, bool isMapSpecific)
        {
            MapId = mapId;
            MessageId = messageId;
            IsMapSpecific = isMapSpecific;
        }
    }
}