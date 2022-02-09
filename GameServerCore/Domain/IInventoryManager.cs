﻿using System.Collections;
using GameServerCore.Domain.GameObjects;
using System.Collections.Generic;
using System;

namespace GameServerCore.Domain
{
    public interface IInventoryManager
    {
        IItem GetItem(byte slot);
        IItem GetItem(string ItemSpellName);
        bool HasItemWithID(int ItemID);
        byte GetItemSlot(IItem item);
        bool RemoveItem(byte slot, IObjAiBase owner = null, int stacksToRemove = 1);
        bool RemoveItem(IItem item, IObjAiBase owner = null, int stacksToRemove = 1);
        KeyValuePair<IItem, bool> AddItem(IItemData item, IObjAiBase owner = null);
        IItem SetExtraItem(byte slot, IItemData item);
        void SwapItems(byte slot1, byte slot2);
        List<IItem> GetAvailableItems(IEnumerable<IItemData> items);
        IEnumerator GetEnumerator();
    }
}
