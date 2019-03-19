namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    class UndoItemRequest : ICoreRequest
    {
        public int NetId { get; }
        public UndoItemRequest(int netId)
        {
            NetId = netId;
        }
    }
}
