using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;

namespace LeagueSandbox.GameServer.Logic.Items
{
    public class Shop
    {
        private Champion _owner;
        private InventoryManager _inventory;

        private Shop(Champion owner)
        {
            _owner = owner;
            _inventory = _owner.Inventory;
        }

        /*
        public bool ItemBuyRequest(int itemId)
        {
            var item = Item.Instantiate(itemId);
            if (item == null)
            {
                return false;
            }

            var recipeParts = _inventory.GetAvailableRecipeParts(item);
            var price = item.TotalPrice;
            Item i;

            if (recipeParts.Length == 0)
            {
                if (_owner.Stats.Gold < price)
                {
                    return true;
                }

                i = _owner.Inventory.AddItem(item);

                if (i == null)
                {
                    return false;
                }
            }
            else
            {
                foreach (var instance in recipeParts)
                {
                    price -= instance.TotalPrice;
                }

                if (_owner.Stats.Gold < price)
                {
                    return false;
                }

                foreach (var instance in recipeParts)
                {
                    _owner.Stats.unapplyStatMods(instance.getTemplate().getStatMods());
                    _game.PacketNotifier.notifyRemoveItem(_owner, instance.getSlot(), 0);
                    _owner.Inventory.RemoveItem(instance.getSlot());
                }

                i = _game.GetPeerInfo(peer).GetChampion().Inventory.AddItem(itemTemplate);
            }

            _owner.Stats.Gold -= price;
            _owner.Stats.pplyStatMods(itemTemplate.getStatMods());
            _game.PacketNotifier.NotifyItemBought(_game.GetPeerInfo(peer).GetChampion(), i);

            return true;
        }
        */

        public static Shop CreateShop(Champion owner)
        {
            return new Shop(owner);
        }
    }
}
