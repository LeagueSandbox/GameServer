using System.Collections.Generic;
using System.Linq;

namespace LeagueSandbox.GameServer.Items
{
    public class ItemRecipe
    {
        private readonly ItemType _itemType;
        private ItemType[] _items;
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

        private ItemRecipe(ItemType itemType, ItemManager manager)
        {
            _itemType = itemType;
            _totalPrice = -1;
            _itemManager = manager;
        }

        public List<ItemType> GetItems()
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
                itemManager.SafeGetItemType(_itemType.RecipeItem1),
                itemManager.SafeGetItemType(_itemType.RecipeItem2),
                itemManager.SafeGetItemType(_itemType.RecipeItem3),
                itemManager.SafeGetItemType(_itemType.RecipeItem4)
            }.Where(i => i != null).ToArray();
        }

        private void FindPrice()
        {
            _totalPrice = 0;
            foreach (var item in GetItems())
            {
                _totalPrice += item.TotalPrice;
            }

            _totalPrice += _itemType.Price;
        }

        public static ItemRecipe FromItemType(ItemType type, ItemManager manager)
        {
            return new ItemRecipe(type, manager);
        }
    }
}