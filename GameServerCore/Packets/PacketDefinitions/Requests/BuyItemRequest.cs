namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class BuyItemRequest : ICoreRequest
    {
        public uint ItemID { get; }

        public BuyItemRequest(uint itemId)
        {
            ItemID = itemId;
        }
    }
}
