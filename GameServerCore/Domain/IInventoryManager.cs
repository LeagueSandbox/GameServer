﻿using System.Collections;
using GameServerCore.Domain.GameObjects;
using System.Collections.Generic;

namespace GameServerCore.Domain
{
    public interface IInventoryManager
    {
        IItem GetItem(byte slot);
        IItem GetItem(string ItemSpellName);
        byte GetItemSlot(IItem item);
        void RemoveItem(byte slot, IObjAiBase owner);
        bool AddItem(IItemData item, IObjAiBase owner = null);
        IItem SetExtraItem(byte slot, IItemData item);
        void SwapItems(byte slot1, byte slot2);
        List<IItem> GetAvailableItems(IEnumerable<IItemData> items);
        IEnumerator GetEnumerator();
    }
}
