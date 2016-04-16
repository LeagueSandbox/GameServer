using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Items
{
    public class InventoryManager
    {
        private Inventory _inventory;
        private Champion _owner;

        private InventoryManager(Game game, Champion owner)
        {
            _owner = owner;
            _inventory = new Inventory(game, this);
        }

        public Item AddItem(ItemType item)
        {
            return _inventory.AddItem(item);
        }

        public Item GetItem(int slot)
        {
            return _inventory.GetItem(slot);
        }

        public void RemoveItem(int slot)
        {
            _inventory.RemoveItem(slot);
        }

        public void SwapItems(int slot1, int slot2)
        {
            _inventory.SwapItems(slot1, slot2);
        }

        public List<Item> GetAvailableItems(ItemRecipe recipe)
        {
            var result = new List<Item>();
            foreach(var item in _inventory.Items)
            {
                if (item == null) continue;
                if (!recipe.Items.Contains(item.ItemType)) continue;
                if (result.Contains(item)) continue;
                result.Add(item);
            }
            return result;
        }

        public static InventoryManager CreateInventory(Game game, Champion owner)
        {
            return new InventoryManager(game, owner);
        }
    }
}
