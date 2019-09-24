using System;
using System.Linq;
using GameServerCore.Domain;

namespace LeagueSandbox.GameServer.Items
{
    public class Inventory
    {
        private const byte TRINKET_SLOT = 6;
        private const byte BASE_INVENTORY_SIZE = 7; // Includes trinket
        private const byte EXTRA_INVENTORY_SIZE = 7;
        private const byte RUNE_INVENTORY_SIZE = 30;
        private InventoryManager _owner;
        public IItem[] Items { get; }

        public Inventory(InventoryManager owner)
        {
            _owner = owner;
            Items = new IItem[BASE_INVENTORY_SIZE + EXTRA_INVENTORY_SIZE + RUNE_INVENTORY_SIZE];
        }

        public IItem[] GetBaseItems()
        {
            return Items.Take(BASE_INVENTORY_SIZE).ToArray();
        }
        
        public IItem AddItem(IItemData item)
        {
            if (item.ItemGroup.ToLower().Equals("relicbase"))
            {
                return AddTrinketItem(item);
            }

            if (item.MaxStack > 1)
            {
                return AddStackingItem(item);
            }

            return AddNewItem(item);
        }

        public IItem SetExtraItem(byte slot, IItemData item)
        {
            if (slot < BASE_INVENTORY_SIZE)
            {
                throw new Exception("Invalid extra item slot—must be greater than base inventory size!");
            }

            return SetItem(slot, item);
        }

        private IItem SetItem(byte slot, IItemData item)
        {
            Items[slot] = Item.CreateFromType(item);
            return Items[slot];
        }

        public IItem GetItem(byte slot)
        {
            return Items[slot];
        }

        public void RemoveItem(byte slot)
        {
            Items[slot] = null;
        }

        public void RemoveItem(IItem item)
        {
            RemoveItem(GetItemSlot(item));
        }

        public byte GetItemSlot(IItem item)
        {
            for (byte i = 0; i < Items.Length; i++)
            {
                if (Items[i] != item)
                {
                    continue;
                }

                return i;
            }

            throw new Exception("Specified item doesn't exist in the inventory!");
        }

        public void SwapItems(byte slot1, byte slot2)
        {
            if (slot1 == TRINKET_SLOT || slot2 == TRINKET_SLOT)
            {
                throw new Exception("Can't swap to or from the trinket slot");
            }

            var buffer = Items[slot1];
            Items[slot1] = Items[slot2];
            Items[slot2] = buffer;
        }

        private IItem AddTrinketItem(IItemData item)
        {
            if (Items[TRINKET_SLOT] != null)
            {
                return null;
            }

            return SetItem(TRINKET_SLOT, item);
        }

        private IItem AddStackingItem(IItemData item)
        {
            for (var i = 0; i < BASE_INVENTORY_SIZE; i++)
            {
                if (Items[i] == null)
                {
                    continue;
                }

                if (item.ItemId != Items[i].ItemData.ItemId)
                {
                    continue;
                }

                if (Items[i].IncrementStackCount())
                {
                    return Items[i];
                }

                return null;
            }
            return AddNewItem(item);
        }

        private IItem AddNewItem(IItemData item)
        {
            for (var i = 0; i < BASE_INVENTORY_SIZE; i++)
            {
                if (i == TRINKET_SLOT)
                {
                    continue;
                }

                if (Items[i] != null)
                {
                    continue;
                }

                return SetItem((byte)i, item);
            }

            return null;
        }
    }
}
