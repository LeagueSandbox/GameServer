using GameServerCore.Enums;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class AnnounceEventResponse : ICoreResponse
    {
        public int MapId { get; }
        public EventID MessageId { get; }
        public bool IsMapSpecific { get; }
        public AnnounceEventResponse(int mapId, EventID messageId, bool isMapSpecific)
        {
            MapId = mapId;
            MessageId = messageId;
            IsMapSpecific = isMapSpecific;
        }
    }
}