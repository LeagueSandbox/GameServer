namespace GameServerCore.Domain
{
    public interface IInventoryManager
    {
        IItem GetItem(int slot);
        void RemoveItem(int slot);
        int GetItemSlot(IItem item);
        IItem SetExtraItem(int slot, IItemType item);
        void SwapItems(int slot1, int slot2);
    }
}
