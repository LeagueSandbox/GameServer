using System.Collections.Generic;
using LeagueSandbox.GameServer.Content;

namespace LeagueSandbox.GameServer.Inventory
{
    public class ItemManager
    {
        private readonly Dictionary<int, ItemData> _itemTypes;

        public ItemManager()
        {
            _itemTypes = new Dictionary<int, ItemData>();
        }

        public ItemData GetItemType(int itemId)
        {
            return _itemTypes[itemId];
        }

        public ItemData SafeGetItemType(int itemId)
        {
            return _itemTypes.GetValueOrDefault(itemId, null);
        }

        public void ResetItems()
        {
            _itemTypes.Clear();
        }

        public void AddItemType(ItemData itemType)
        {
            _itemTypes.Add(itemType.ItemId, itemType);
            itemType.CreateRecipe(this);
        }

        public void AddItems(ItemContentCollection contentCollection)
        {
            foreach (var entry in contentCollection)
            {
                var itemType = (new ItemData()).Load(entry.Value);
                _itemTypes.Add(entry.Key, itemType);
                itemType.CreateRecipe(this);
            }
        }
    }
}