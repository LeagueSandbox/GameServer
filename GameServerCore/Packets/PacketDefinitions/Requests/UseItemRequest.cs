namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    class UseItemRequest : ICoreRequest
    {
        public int NetId { get; }
        public byte SlotId { get; }

        public UseItemRequest(int netId, byte slotId)
        {
            NetId = netId;
            SlotId = slotId;
        }
    }
}
