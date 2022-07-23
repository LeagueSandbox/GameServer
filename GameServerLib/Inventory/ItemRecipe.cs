using System.Collections.Generic;
using System.Linq;

namespace LeagueSandbox.GameServer.Inventory
{
    public class ItemRecipe
    {
        private readonly ItemData _itemData;
        private ItemData[] _items;
        private int _totalPrice;
        private readonly ItemManager _itemManager;

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

        private ItemRecipe(ItemData itemData, ItemManager manager)
        {
            _itemData = itemData;
            _totalPrice = -1;
            _itemManager = manager;
        }

        public IEnumerable<ItemData> GetItems()
        {
            if (_items == null)
            {
                FindRecipeItems(_itemManager);
            }

            return _items;
        }

        private void FindRecipeItems(ItemManager itemManager)
        {
            _items = _itemData.RecipeItem.AsEnumerable()
                .Select(itemManager.SafeGetItemType)
                .Where(i => i != null).ToArray();
        }

        private void FindPrice()
        {
            _totalPrice = 0;
            foreach (var item in GetItems())
            {
                _totalPrice += item.TotalPrice;
            }

            _totalPrice += _itemData.Price;
        }

        public static ItemRecipe FromItemType(ItemData data, ItemManager manager)
        {
            return new ItemRecipe(data, manager);
        }
    }
}