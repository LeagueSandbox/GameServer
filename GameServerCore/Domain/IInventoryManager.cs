using System.Collections;
using System.Collections.Generic;

namespace GameServerCore.Domain
{
    public interface IInventoryManager
    {
        IItem GetItem(byte slot);
        byte GetItemSlot(IItem item);
        void RemoveItem(byte slot);
        void RemoveItem(IItem item);
        IItem AddItem(IItemData item);
        IItem SetExtraItem(byte slot, IItemData item);
        void SwapItems(byte slot1, byte slot2);
        List<IItem> GetAvailableItems(IEnumerable<IItemData> items);
        IEnumerator GetEnumerator();
    }
}
