using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;

namespace LeagueSandbox.GameServer.Logic.Items
{
    public class InventoryManager
    {
        private Inventory _inventory;
        private AttackableUnit _owner;

        private InventoryManager(AttackableUnit owner)
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

        public static InventoryManager CreateInventory(AttackableUnit owner)
        {
            return new InventoryManager(owner);
        }

        public IEnumerator GetEnumerator()
        {
            return _inventory.Items.GetEnumerator();
        }
    }
}
