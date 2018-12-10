namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class BuyItemRequest : ICoreRequest
    {
        public int NetId { get; }
        public int ItemId { get; }

        public BuyItemRequest(int netId, int itemId)
        {
            NetId = netId;
            ItemId = itemId;
        }
    }
}
