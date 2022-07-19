using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logging;
using log4net;
using PacketDefinitions420;

namespace LeagueSandbox.GameServer.Inventory
{
    public class InventoryManager
    {
        private readonly PacketNotifier _packetNotifier;
        private readonly Inventory _inventory;
        private static ILog _logger = LoggerProvider.GetLogger();

        private InventoryManager(PacketNotifier packetNotifier)
        {
            _packetNotifier = packetNotifier;
            _inventory = new Inventory(this);
        }

        public KeyValuePair<Item, bool> AddItem(ItemData itemData, ObjAIBase owner = null)
        {
            var item = _inventory.AddItem(itemData, owner);

            if (item == null)
            {
                return KeyValuePair.Create(item, false);
            }

            if (owner != null && item != null)
            {
                //This packet seems to break when buying more than 3 of one of the 250Gold elixirs
                _packetNotifier.NotifyBuyItem(owner, item);
            }
            return KeyValuePair.Create(item, true);
        }

        public KeyValuePair<Item, bool> AddItemToSlot(ItemData itemData, ObjAIBase owner, byte slot)
        {
            var item = _inventory.SetItemToSlot(itemData, owner, slot);

            if (item == null)
            {
                return KeyValuePair.Create(item, false);
            }

            if (owner != null && item != null)
            {
                //This packet seems to break when buying more than 3 of one of the 250Gold elixirs
                _packetNotifier.NotifyBuyItem(owner, item);
            }

            return KeyValuePair.Create(item, true);
        }

        public Item SetExtraItem(byte slot, ItemData item)
        {
            return _inventory.SetExtraItem(slot, item);
        }

        public Item GetItem(byte slot)
        {
            return _inventory.GetItem(slot);
        }

        public Item GetItem(string itemSpellName)
        {
            return _inventory.GetItem(itemSpellName);
        }
        public List<Item> GetAllItems(bool includeRunes = false, bool includeRecallItem = false)
        {
            List<Item> toReturn = new List<Item>(_inventory.Items.ToList());
            if (!includeRecallItem)
            {
                toReturn.RemoveAt(7);
            }
            toReturn.RemoveAll(x => x == null);
            if (!includeRunes)
            {
                toReturn.RemoveAll(x => x.ItemData.ItemId >= 5000);
            }

            return toReturn;
        }

        public bool HasItemWithID(int ItemID)
        {
            return _inventory.HasItemWithID(ItemID);
        }

        public bool RemoveItem(byte slot, ObjAIBase owner = null, int stacksToRemove = 1)
        {
            var item = _inventory.Items[slot];
            if (item == null)
            {
                return false;
            }

            _inventory.RemoveItem(slot, owner, stacksToRemove);

            if (owner != null)
            {
                item = _inventory.Items[slot];

                byte stacks = 0;
                if (item != null)
                {
                    stacks = (byte)item.StackCount;
                }

                _packetNotifier.NotifyRemoveItem(owner, slot, stacks);
            }

            return true;
        }
        public bool RemoveItem(Item item, ObjAIBase owner = null, int stacksToRemove = 1)
        {
            var slot = _inventory.GetItemSlot(item);

            if (_inventory.Items[slot] == null)
            {
                return false;
            }

            _inventory.RemoveItem(slot, owner, stacksToRemove);

            if (owner != null)
            {
                _packetNotifier.NotifyRemoveItem(owner, slot, (byte)item.StackCount);
            }

            return true;
        }
        public byte GetItemSlot(Item item)
        {
            return _inventory.GetItemSlot(item);
        }

        public void SwapItems(byte slot1, byte slot2)
        {
            _inventory.SwapItems(slot1, slot2);
        }

        public List<Item> GetAvailableItems(IEnumerable<ItemData> items)
        {
            var tempInv = new List<Item>(_inventory.GetBaseItems());
            return GetAvailableItemsRecursive(ref tempInv, items);
        }

        private static List<Item> GetAvailableItemsRecursive(ref List<Item> inventoryState, IEnumerable<ItemData> items)
        {
            var result = new List<Item>();
            foreach (var component in items)
            {
                if (component == null)
                {
                    continue;
                }
                var idx = inventoryState.FindIndex(i => i != null && i.ItemData == component);
                if (idx == -1)
                {
                    result = result.Concat(GetAvailableItemsRecursive(ref inventoryState, component.Recipe.GetItems())).ToList();
                }
                else
                {
                    result.Add(inventoryState[idx]);
                    // remove entry in case that the recipe has the same item more than once in it
                    inventoryState[idx] = null;
                }
            }
            return result;
        }

        public static InventoryManager CreateInventory(PacketNotifier packetNotifier)
        {
            return new InventoryManager(packetNotifier);
        }

        public IEnumerator GetEnumerator()
        {
            return _inventory.Items.GetEnumerator();
        }

        public void OnUpdate(float diff)
        {
            foreach (var item in _inventory.ItemScripts)
            {
                try
                {
                    item.Value.OnUpdate(diff);
                }
                catch(Exception e)
                {
                    _logger.Error(null, e);
                }
            }
        }
    }
}
