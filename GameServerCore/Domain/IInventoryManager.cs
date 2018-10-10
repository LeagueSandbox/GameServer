namespace GameServerCore.Domain
{
    public interface IInventoryManager
    {
        IItem GetItem(byte slot);
        void RemoveItem(byte slot);
        byte GetItemSlot(IItem item);
        IItem SetExtraItem(byte slot, IItemData item);
        void SwapItems(byte slot1, byte slot2);
    }
}
