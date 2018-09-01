using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Content;
using GameServerCore.Domain.GameObjects;

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

        public bool ItemBuyRequest(int itemId)
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
            Item i;

            if (ownedItems.Count == 0)
            {
                if (stats.Gold < price || (i = inventory.AddItem(itemTemplate)) == null)
                {
                    return false;
                }
            }
            else
            {
                ownedItems.ForEach(item => price -= item.ItemType.TotalPrice);
                if (stats.Gold < price)
                {
                    return false;
                }
                
                foreach (var item in ownedItems)
                {
                    stats.RemoveModifier(item.ItemType);
                    _game.PacketNotifier.NotifyRemoveItem(_owner, inventory.GetItemSlot(item), 0);
                    _owner.RemoveSpell((byte)(inventory.GetItemSlot(item) + ITEM_ACTIVE_OFFSET));
                    inventory.RemoveItem(item);
                }

                i = inventory.AddItem(itemTemplate);
            }
            
            stats.Gold -= price;
            stats.AddModifier(itemTemplate);
            _game.PacketNotifier.NotifyItemBought(_owner, i);

            if (!string.IsNullOrEmpty(i.ItemType.SpellName))
            {
                _owner.SetSpell(i.ItemType.SpellName, (byte)(inventory.GetItemSlot(i) + ITEM_ACTIVE_OFFSET), true);
            }

            return true;
        }

        public static Shop CreateShop(Champion owner, Game game)
        {
            return new Shop(owner, game);
        }
    }
}
