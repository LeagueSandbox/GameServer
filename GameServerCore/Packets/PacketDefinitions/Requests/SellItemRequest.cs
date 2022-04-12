namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class SellItemRequest : ICoreRequest
    {
        public byte Slot { get; }
        public bool Sell { get; }

        public SellItemRequest(byte slot, bool sell)
        {
            Slot = slot;
            Sell = sell;
        }
    }
}
