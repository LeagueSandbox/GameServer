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
        private Item[] _items;
        private InventoryManager _owner;
        private Game _game;

        public Item[] Items { get { return _items; } }

        public Inventory(Game game, InventoryManager owner)
        {
            _game = game;
            _owner = owner;
            _items = new Item[7];
        }

        public Item AddItem(ItemType item)
        {
            if(item.GetIsTrinket()) return AddTrinketItem(item);
            if (item.MaxStack > 1) return AddStackingItem(item);
            return AddNewItem(item);
        }

        public Item SetItem(byte slot, ItemType item)
        {
            if (slot > _items.Length) return null;
            _items[slot] = Item.CreateFromType(_game, this, item, slot);
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

        public void SwapItems(int slot1, int slot2)
        {
            if (slot1 == TRINKET_SLOT || slot2 == TRINKET_SLOT)
                throw new Exception("Can't swap to or from the trinket slot");

            var buffer = _items[slot1];
            _items[slot1] = _items[slot2];
            _items[slot2] = buffer;
            ChangeItemSlotValue(slot1, (byte)slot1);
            ChangeItemSlotValue(slot2, (byte)slot2);
        }

        private void ChangeItemSlotValue(int itemSlot, byte value)
        {
            if (_items[itemSlot] == null) return;
            _items[itemSlot].SetSlot(value);
        }

        private Item AddTrinketItem(ItemType item)
        {
            if (_items[TRINKET_SLOT] != null) return null;
            return SetItem(TRINKET_SLOT, item);
        }

        private Item AddStackingItem(ItemType item)
        {
            for(var i = 0; i < _items.Length; i++)
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
            for(var i = 0; i < _items.Length; i++)
            {
                if (i == TRINKET_SLOT) continue;
                if (_items[i] != null) continue;
                return SetItem((byte)i, item);
            }
            return null;
        }
    }
}
