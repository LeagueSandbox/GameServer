using System.Collections;
using GameServerCore.Domain.GameObjects;
using System.Collections.Generic;
using System;

namespace GameServerCore.Domain
{
    public interface IInventoryManager
    {
        IItem GetItem(byte slot);
        IItem GetItem(string ItemSpellName);
        List<IItem> GetAllItems(bool includeRunes = false);
        bool HasItemWithID(int ItemID);
        byte GetItemSlot(IItem item);
        bool RemoveItem(byte slot, IObjAiBase owner = null, int stacksToRemove = 1);
        bool RemoveItem(IItem item, IObjAiBase owner = null, int stacksToRemove = 1);
        KeyValuePair<IItem, bool> AddItem(IItemData item, IObjAiBase owner = null);
        KeyValuePair<IItem, bool> AddItemToSlot(IItemData itemData, IObjAiBase owner, byte slot);
        IItem SetExtraItem(byte slot, IItemData item);
        void SwapItems(byte slot1, byte slot2);
        List<IItem> GetAvailableItems(IEnumerable<IItemData> items);
        IEnumerator GetEnumerator();
        void OnUpdate(float diff);
    }
}
