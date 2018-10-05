namespace GameServerCore.Domain
{
    public interface IShop
    {
        bool HandleItemSellRequest(byte slotId);
        bool HandleItemBuyRequest(int itemId);
    }
}