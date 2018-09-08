using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameServerCore.Domain;

namespace LeagueSandbox.GameServer.Items
{
    public class InventoryManager : IInventoryManager
    {
        private readonly Inventory _inventory;

        private InventoryManager()
        {
            _inventory = new Inventory(this);
        }

        public Item AddItem(ItemType item)
        {
            return _inventory.AddItem(item);
        }

        public Item SetExtraItem(byte slot, ItemType item)
        {
            return _inventory.SetExtraItem(slot, item);
        }

        public Item GetItem(byte slot)
        {
            return _inventory.GetItem(slot);
        }

        public void RemoveItem(byte slot)
        {
            _inventory.RemoveItem(slot);
        }

        public void RemoveItem(Item item)
        {
            _inventory.RemoveItem(item);
        }

        public byte GetItemSlot(Item item)
        {
            return _inventory.GetItemSlot(item);
        }

        public void SwapItems(byte slot1, byte slot2)
        {
            _inventory.SwapItems(slot1, slot2);
        }

        public List<Item> GetAvailableItems(ItemRecipe recipe)
        {
            var tempInv = new List<Item>(_inventory.GetBaseItems());
            return GetAvailableItemsRecursive(ref tempInv, recipe);
        }
        
        private static List<Item> GetAvailableItemsRecursive(ref List<Item> inventoryState, ItemRecipe recipe)
        {
            var result = new List<Item>();
            var tmpRecipe = recipe.GetItems();
            foreach (var component in tmpRecipe)
            {
                if (component == null)
                {
                    continue;
                }
                var idx = inventoryState.FindIndex(i => i != null && i.ItemType == component);
                if (idx == -1)
                {
                    result = result.Concat(GetAvailableItemsRecursive(ref inventoryState, component.Recipe)).ToList();
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

        public static InventoryManager CreateInventory()
        {
            return new InventoryManager();
        }

        public IEnumerator GetEnumerator()
        {
            return _inventory.Items.GetEnumerator();
        }

        byte IInventoryManager.GetItemSlot(IItem item)
        {
            return GetItemSlot((Item)item);
        }

        IItem IInventoryManager.SetExtraItem(byte slot, IItemType item)
        {
            return SetExtraItem(slot, (ItemType)item);
        }

        IItem IInventoryManager.GetItem(byte slot)
        {
            return GetItem(slot);
        }
    }
}
