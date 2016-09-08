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
    public class Inventory
    {
        private const int TRINKET_SLOT = 6;
        private const int BASE_INVENTORY_SIZE = 7; // Includes trinket
        private const int EXTRA_INVENTORY_SIZE = 7;
        private const int RUNE_INVENTORY_SIZE = 30;
        private Item[] _items;
        private InventoryManager _owner;
        public Item[] Items { get { return _items; } }

        public Inventory(InventoryManager owner)
        {
            _owner = owner;
            _items = new Item[BASE_INVENTORY_SIZE + EXTRA_INVENTORY_SIZE + RUNE_INVENTORY_SIZE];
        }

        public Item AddItem(ItemType item)
        {
            if(item.GetIsTrinket()) return AddTrinketItem(item);
            if (item.MaxStack > 1) return AddStackingItem(item);
            return AddNewItem(item);
        }

        public Item SetExtraItem(byte slot, ItemType item)
        {
            if (slot < BASE_INVENTORY_SIZE)
                throw new Exception("Invalid extra item slot—must be greater than base inventory size!");
            return SetItem(slot, item);
        }

        private Item SetItem(byte slot, ItemType item)
        {
            _items[slot] = Item.CreateFromType(this, item);
            return _items[slot];
        }

        public Item GetItem(int slot)
        {
            return _items[slot];
        }

        public void RemoveItem(int slot)
        {
            _items[slot] = null;
        }

        public void RemoveItem(Item item)
        {
            RemoveItem(GetItemSlot(item));
        }

        public int GetItemSlot(Item item)
        {
            for(var i = 0; i < _items.Length; i++)
            {
                if (_items[i] != item) continue;
                return i;
            }
            throw new Exception("Specified item doesn't exist in the inventory!");
        }

        public void SwapItems(int slot1, int slot2)
        {
            if (slot1 == TRINKET_SLOT || slot2 == TRINKET_SLOT)
                throw new Exception("Can't swap to or from the trinket slot");

            var buffer = _items[slot1];
            _items[slot1] = _items[slot2];
            _items[slot2] = buffer;
        }

        private Item AddTrinketItem(ItemType item)
        {
            if (_items[TRINKET_SLOT] != null) return null;
            return SetItem(TRINKET_SLOT, item);
        }

        private Item AddStackingItem(ItemType item)
        {
            for(var i = 0; i < BASE_INVENTORY_SIZE; i++)
            {
                if (_items[i] == null) continue;
                if (item.ItemId != _items[i].ItemType.ItemId) continue;
                if (_items[i].IncrementStackSize()) return _items[i];
                return null;
            }
            return AddNewItem(item);
        }

        private Item AddNewItem(ItemType item)
        {
            for(var i = 0; i < BASE_INVENTORY_SIZE; i++)
            {
                if (i == TRINKET_SLOT) continue;
                if (_items[i] != null) continue;
                return SetItem((byte)i, item);
            }
            return null;
        }
    }
}
