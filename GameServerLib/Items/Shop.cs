using System.Linq;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace LeagueSandbox.GameServer.Items
{
    public class Shop
    {
        private readonly Champion _owner;
        private readonly Game _game;

        public const byte ITEM_ACTIVE_OFFSET = 6;

        private Shop(Champion owner, Game game)
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

            var sellPrice = i.TotalPrice * i.ItemData.SellBackModifier;
            _owner.Stats.Gold += sellPrice;

            if (i.ItemData.MaxStack > 1)
            {
                i.DecrementStackSize();
            }
            RemoveItem(i, slotId, i.StackSize);
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
            var ownedItems = inventory.GetAvailableItems(itemTemplate.Recipe);

            if (ownedItems.Count != 0)
            {
                price -= ownedItems.Sum(item => item.ItemData.TotalPrice);
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

        private void RemoveItem(Item item, byte slotId, byte stackSize = 0)
        {
            var inventory = _owner.Inventory;
            _owner.Stats.RemoveModifier(item.ItemData);
            _game.PacketNotifier.NotifyRemoveItem(_owner, slotId, stackSize);
            _owner.RemoveSpell((byte)(slotId + ITEM_ACTIVE_OFFSET));
            inventory.RemoveItem(item);
        }

        private bool AddItem(ItemData ıtemData)
        {
            var i = _owner.Inventory.AddItem(ıtemData);
            if (i == null)
            {
                return false;
            }
            _owner.Stats.AddModifier(ıtemData);
            _game.PacketNotifier.NotifyItemBought(_owner, i);
            if (!string.IsNullOrEmpty(i.ItemData.SpellName))
            {
                _owner.SetSpell(i.ItemData.SpellName, (byte)(_owner.Inventory.GetItemSlot(i) + ITEM_ACTIVE_OFFSET), true);
            }
            return true;
        }

        public static Shop CreateShop(Champion owner, Game game)
        {
            return new Shop(owner, game);
        }
    }
}
