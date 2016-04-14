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

        private InventoryManager(Champion owner)
        {
            _owner = owner;
            _inventory = new Inventory();
        }

        public ItemInstance AddItem(ItemTemplate item)
        {
            return _inventory.addItem(item);
        }

        public ItemInstance GetItem(int slot)
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

        public ItemInstance[] GetAvailableRecipeParts(ItemTemplate item)
        {
            return _inventory.getAvailableRecipeParts(item).ToArray();
        }

        public static InventoryManager CreateInventory(Champion owner)
        {
            return new InventoryManager(owner);
        }
    }
}
