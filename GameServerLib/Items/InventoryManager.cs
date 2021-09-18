using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;

namespace LeagueSandbox.GameServer.Items
{
    public class InventoryManager : IInventoryManager
    {
        private readonly Game _game;
        private readonly Inventory _inventory;

        private InventoryManager(Game game)
        {
            _game = game;
            _inventory = new Inventory(this);
        }

        public IItem AddItem(IItemData item)
        {
            return _inventory.AddItem(item);
        }

        public IItem SetExtraItem(byte slot, IItemData item)
        {
            return _inventory.SetExtraItem(slot, item);
        }

        public IItem GetItem(byte slot)
        {
            return _inventory.GetItem(slot);
        }

        public IItem GetItem(string itemSpellName)
        {
            for (byte i = 0; i < _inventory.Items.Length - 1; i++)
            {
                if (_inventory.Items[i] != null)
                {
                    if (itemSpellName == _inventory.Items[i].ItemData.SpellName)
                    {
                        return _inventory.Items[i];
                    }
                }
            }
            return _inventory.GetItem(itemSpellName);
        }
        public void RemoveItem(byte slot)
        {
            _inventory.RemoveItem(slot);
        }

        public void RemoveItem(IItem item)
        {
            _inventory.RemoveItem(item);
        }

        public void RemoveStackingItem(string itemSpellName, IObjAiBase owner)
        {
            _inventory.RemoveStackingItem(_game, itemSpellName, owner);
        }
        public byte GetItemSlot(IItem item)
        {
            return _inventory.GetItemSlot(item);
        }

        public void SwapItems(byte slot1, byte slot2)
        {
            _inventory.SwapItems(slot1, slot2);
        }

        public List<IItem> GetAvailableItems(IEnumerable<IItemData> items)
        {
            var tempInv = new List<IItem>(_inventory.GetBaseItems());
            return GetAvailableItemsRecursive(ref tempInv, items);
        }

        private static List<IItem> GetAvailableItemsRecursive(ref List<IItem> inventoryState, IEnumerable<IItemData> items)
        {
            var result = new List<IItem>();
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

        public static InventoryManager CreateInventory(Game game)
        {
            return new InventoryManager(game);
        }

        public IEnumerator GetEnumerator()
        {
            return _inventory.Items.GetEnumerator();
        }
    }
}
