namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class QuestClickedRequest
    {
        public uint QuestNetId { get; }

        public QuestClickedRequest(uint questNetId)
        {
            QuestNetId = questNetId;
        }
    }
}
