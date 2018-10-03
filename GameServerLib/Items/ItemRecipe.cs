using System.Collections.Generic;
using System.Linq;

namespace LeagueSandbox.GameServer.Items
{
    public class ItemRecipe
    {
        private ItemData _owner;
        private ItemData[] _items;
        private int _totalPrice;
        private ItemManager _itemManager;

        public int TotalPrice
        {
            get
            {
                if (_totalPrice <= -1)
                {
                    FindPrice();
                }

                return _totalPrice;
            }
        }

        private ItemRecipe(ItemData owner, ItemManager manager)
        {
            _owner = owner;
            _totalPrice = -1;
            _itemManager = manager;
        }

        public List<ItemData> GetItems()
        {
            if (_items == null)
            {
                FindRecipeItems(_itemManager);
            }

            return _items.ToList();
        }

        private void FindRecipeItems(ItemManager itemManager)
        {
            // TODO: Figure out how to refactor this.
            _items = new[]
            {
                itemManager.SafeGetItemType(_owner.RecipeItem1),
                itemManager.SafeGetItemType(_owner.RecipeItem2),
                itemManager.SafeGetItemType(_owner.RecipeItem3),
                itemManager.SafeGetItemType(_owner.RecipeItem4)
            };
        }

        private void FindPrice()
        {
            _totalPrice = 0;
            foreach (var item in GetItems())
            {
                if (item != null)
                {
                    _totalPrice += item.TotalPrice;
                }
            }

            _totalPrice += _owner.Price;
        }

        public static ItemRecipe FromItemType(ItemData type, ItemManager manager)
        {
            return new ItemRecipe(type, manager);
        }
    }
}