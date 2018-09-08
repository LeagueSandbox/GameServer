namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    class UndoItemRequest
    {
        public int NetId { get; }
        public UndoItemRequest(int netId)
        {
            NetId = netId;
        }
    }
}
