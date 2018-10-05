using System.Linq;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;

namespace LeagueSandbox.GameServer.Items
{
    public class Shop: IShop
    {
        private readonly IChampion _owner;
        private readonly Game _game;

        public const byte ITEM_ACTIVE_OFFSET = 6;
        
        private Shop(IChampion owner, Game game)
        {
            _owner = owner;
            _game = game;
        }

        public bool HandleItemSellRequest(byte slotId)
        {
            var inventory = _owner.Inventory;
            var i = inventory.GetItem(slotId);
            if (i == null)
            {
                return false;
            }

            var sellPrice = i.TotalPrice * i.ItemType.SellBackModifier;
            _owner.Stats.Gold += sellPrice;

            if (i.ItemType.MaxStack > 1)
            {
                i.DecrementStackCount();
            }
            RemoveItem(i, slotId, i.StackCount);
            return true;
        }

        public bool HandleItemBuyRequest(int itemId)
        {
            var itemTemplate = _game.ItemManager.SafeGetItemType(itemId);
            if (itemTemplate == null)
            {
                return false;
            }

            var stats = _owner.Stats;
            var inventory = _owner.Inventory;
            var price = itemTemplate.TotalPrice;
            var ownedItems = inventory.GetAvailableItems(itemTemplate.Recipe.GetItems());

            if (ownedItems.Count != 0)
            {
                price -= ownedItems.Sum(item => item.ItemType.TotalPrice);
                if (stats.Gold < price)
                {
                    return false;
                }

                foreach (var item in ownedItems)
                {
                    RemoveItem(item, inventory.GetItemSlot(item));
                }

                AddItem(itemTemplate);
            }
            else if (stats.Gold < price || !AddItem(itemTemplate))
            {
                return false;
            }
            
            stats.Gold -= price;
            return true;
        }

        private void RemoveItem(IItem item, byte slotId, byte stackSize = 0)
        {
            var inventory = _owner.Inventory;
            _owner.Stats.RemoveModifier(item.ItemType);
            _game.PacketNotifier.NotifyRemoveItem(_owner, slotId, stackSize);
            _owner.RemoveSpell((byte)(slotId + ITEM_ACTIVE_OFFSET));
            inventory.RemoveItem(item);
        }

        private bool AddItem(IItemType itemType)
        {
            var i = _owner.Inventory.AddItem(itemType);
            if (i == null)
            {
                return false;
            }
            _owner.Stats.AddModifier(itemType);
            _game.PacketNotifier.NotifyItemBought(_owner, i);
            if (!string.IsNullOrEmpty(i.ItemType.SpellName))
            {
                _owner.SetSpell(i.ItemType.SpellName, (byte)(_owner.Inventory.GetItemSlot(i) + ITEM_ACTIVE_OFFSET), true);
            }
            return true;
        }

        public static Shop CreateShop(IChampion owner, Game game)
        {
            return new Shop(owner, game);
        }
    }
}
