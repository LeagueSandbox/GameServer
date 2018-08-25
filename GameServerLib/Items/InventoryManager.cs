using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CommandLine;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Content;

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

        public Item GetItem(int slot)
        {
            return _inventory.GetItem(slot);
        }

        public void RemoveItem(int slot)
        {
            _inventory.RemoveItem(slot);
        }

        public void RemoveItem(Item item)
        {
            _inventory.RemoveItem(item);
        }

        public byte GetItemSlot(Item item)
        {
            return (byte)_inventory.GetItemSlot(item);
        }

        public void SwapItems(int slot1, int slot2)
        {
            _inventory.SwapItems(slot1, slot2);
        }

        public List<Item> GetAvailableItems(ItemRecipe recipe)
        {
            // todo is it possible to do this without clonging somehow?
            var tmpInv = (IEnumerable<Item>)_inventory.Items.Clone();
            var result = new List<Item>();
            var tmpRecipe = recipe.GetItems();
            foreach (var component in tmpRecipe)
            {
                if (component == null)
                {
                    continue;
                }
                var item = tmpInv.FirstOrDefault(i => i != null && i.ItemType == component);
                if (item == null)
                {
                    result = result.Concat(GetAvailableItems(component.Recipe)).ToList();
                }
                else
                {
                    result.Add(item);
                    // remove entry in case that the recipe has the same item more than once in it
                    tmpInv = tmpInv.Where(i => i != item);
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

        IItem IInventoryManager.GetItem(int slot)
        {
            return GetItem(slot);
        }
    }
}
