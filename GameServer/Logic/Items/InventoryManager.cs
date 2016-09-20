using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Items
{
    public class InventoryManager
    {
        private Inventory _inventory;
        private Unit _owner;

        private InventoryManager(Unit owner)
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
            var result = new List<Item>();
            var tmpRecipe = recipe.GetItems().ToList();
            foreach(var item in _inventory.Items)
            {
                if (item == null) continue;
                if (!tmpRecipe.Contains(item.ItemType)) continue;
                result.Add(item);
                tmpRecipe.Remove(item.ItemType);
            }
            return result;
        }

        public static InventoryManager CreateInventory(Unit owner)
        {
            return new InventoryManager(owner);
        }

        public IEnumerator GetEnumerator()
        {
            return _inventory.Items.GetEnumerator();
        }
    }
}
