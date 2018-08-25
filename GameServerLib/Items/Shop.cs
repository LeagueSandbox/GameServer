using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Content;

namespace LeagueSandbox.GameServer.Items
{
    public class Shop
    {
        private readonly Champion _owner;
        private readonly Game _game;
        
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
                    inventory.RemoveItem(item);
                }

                i = inventory.AddItem(itemTemplate);
            }
            
            stats.Gold -= price;
            stats.AddModifier(itemTemplate);
            _game.PacketNotifier.NotifyItemBought(_owner, i);
            return true;
        }

        public static Shop CreateShop(Champion owner, Game game)
        {
            return new Shop(owner, game);
        }
    }
}
