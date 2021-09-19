using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Interfaces;

namespace LeagueSandbox.GameServer.Items
{
    public class InventoryManager : IInventoryManager
    {
        private readonly IPacketNotifier _packetNotifier;
        private readonly Inventory _inventory;

        private InventoryManager(IPacketNotifier packetNotifier)
        {
            _packetNotifier = packetNotifier;
            _inventory = new Inventory(this);
        }

        public bool AddItem(IItemData itemData, IObjAiBase owner = null)
        {
            var item = _inventory.AddItem(itemData, owner);
            
            if(item == null)
            {
                return false;
            }

            if (owner is IChampion champion && item != null)
            {
                //This packet seems to break when buying more than 3 of one of the 250Gold elixirs
                _packetNotifier.NotifyBuyItem((int)champion.GetPlayerId(), champion, item);
            }
            return true;
        }

        public IItem SetExtraItem(byte slot, IItemData item)
        {
            return _inventory.SetExtraItem(slot, item);
        }

        public IItem GetItem(byte slot)
        {
            return _inventory.GetItem(slot);
        }

        public IItem GetItem(string itemSpellName)
        {
            return _inventory.GetItem(itemSpellName);
        }
        public void RemoveItem(byte slot, IObjAiBase owner)
        {
            var item = GetItem(slot);
            _inventory.RemoveItem(slot, owner);
            _packetNotifier.NotifyRemoveItem(owner, slot, (byte)item.StackCount);
        }
        public byte GetItemSlot(IItem item)
        {
            return _inventory.GetItemSlot(item);
        }

        public void SwapItems(byte slot1, byte slot2)
        {
            _inventory.SwapItems(slot1, slot2);
        }

        public List<IItem> GetAvailableItems(IEnumerable<IItemData> items)
        {
            var tempInv = new List<IItem>(_inventory.GetBaseItems());
            return GetAvailableItemsRecursive(ref tempInv, items);
        }

        private static List<IItem> GetAvailableItemsRecursive(ref List<IItem> inventoryState, IEnumerable<IItemData> items)
        {
            var result = new List<IItem>();
            foreach (var component in items)
            {
                if (component == null)
                {
                    continue;
                }
                var idx = inventoryState.FindIndex(i => i != null && i.ItemData == component);
                if (idx == -1)
                {
                    result = result.Concat(GetAvailableItemsRecursive(ref inventoryState, component.Recipe.GetItems())).ToList();
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

        public static InventoryManager CreateInventory(IPacketNotifier packetNotifier)
        {
            return new InventoryManager(packetNotifier);
        }

        public IEnumerator GetEnumerator()
        {
            return _inventory.Items.GetEnumerator();
        }
    }
}
