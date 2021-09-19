using System;
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

            var sellPrice = i.TotalPrice * i.ItemData.SellBackModifier;
            _owner.Stats.Gold += sellPrice;

            if (i.ItemData.Consumed)
            {
                _owner.Inventory.RemoveStackingItem(i, _owner);
            }
            else
            {
                _owner.Inventory.RemoveItem(slotId);
            }
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
                price -= ownedItems.Sum(item => item.ItemData.TotalPrice);
                if (stats.Gold < price)
                {
                    return false;
                }

                foreach (var itens in ownedItems)
                {
                    _owner.Inventory.RemoveItem(inventory.GetItemSlot(itens));
                }
                
               _owner.Inventory.AddItem(itemTemplate, _owner);
            }
            else if (stats.Gold < price || _owner.Inventory.AddItem(itemTemplate, _owner) == null)
            {
                return false;
            }
            
            stats.Gold -= price;
            return true;
        }

        public static Shop CreateShop(IChampion owner, Game game)
        {
            return new Shop(owner, game);
        }
    }
}
