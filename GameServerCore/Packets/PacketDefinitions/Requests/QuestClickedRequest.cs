namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class QuestClickedRequest : ICoreRequest
    {
        public uint QuestNetId { get; }

        public QuestClickedRequest(uint questNetId)
        {
            QuestNetId = questNetId;
        }
    }
}
