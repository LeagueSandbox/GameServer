namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class SwapItemsRequest : ICoreRequest
    {
        public int NetId { get; }
        public byte SlotFrom { get; }
        public byte SlotTo { get; }

        public SwapItemsRequest(int netId, byte slotFrom, byte slotTo)
        {
            NetId = netId;
            SlotFrom = slotFrom;
            SlotTo = slotTo;
        }
    }
}
