using System.Collections;
using System.Collections.Generic;

namespace GameServerCore.Domain
{
    public interface IInventoryManager
    {
        IItem GetItem(byte slot);
        void RemoveItem(byte slot);
        void RemoveItem(IItem item);
        IItem AddItem(IItemType item);
        byte GetItemSlot(IItem item);
        IItem SetExtraItem(byte slot, IItemType item);
        void SwapItems(byte slot1, byte slot2);
        List<IItem> GetAvailableItems(IItemRecipe recipe);
        IEnumerator GetEnumerator();
    }
}
