using System;
using System.Collections.Generic;
using System.Linq;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Packets.Interfaces;

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

        public IItem AddItem(IItemData item, IObjAiBase owner)
        {
            if (item.ItemGroup.ToLower().Equals("relicbase"))
            {
                return AddTrinketItem(item);
            }

            if (owner != null)
            {
                owner.Stats.AddModifier(item);

                if (!string.IsNullOrEmpty(item.SpellName))
                {
                    owner.SetSpell(item.SpellName, (byte)(owner.Inventory.GetItemSlot(GetItem(item.SpellName)) + (byte)SpellSlotType.InventorySlots), true);
                }
            }

            if (item.MaxStacks > 1)
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

        public IItem GetItem(string name, bool isItemName = false)
        {
            if (name != null)
            {
                for (byte i = 0; i < Items.Length; i++)
                {
                    if (Items[i] != null)
                    {
                        if (isItemName)
                        {
                            if (name == Items[i].ItemData.Name)
                            {
                                return Items[i];
                            };
                        }
                        else if (name == Items[i].ItemData.SpellName)
                        {
                            return Items[i];
                        }
                    }
                }
            }
            return null;
        }

        public void RemoveItem(byte slot, IObjAiBase owner, int stacksToRemove = 1)
        {
            if (stacksToRemove < 0)
            {
                throw new Exception("Stacks to be Removed can't be a negative number!");
            }

            int finalStacks = Items[slot].StackCount - stacksToRemove;

            if (finalStacks <= 0)
            {
                if (owner != null)
                {
                    owner.Stats.RemoveModifier(Items[slot].ItemData);
                }
                Items[slot] = null;
            }
            else
            {
                Items[slot].SetStacks(finalStacks);
            }
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
