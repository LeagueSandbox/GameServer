using System.Collections.Generic;
using LeagueSandbox.GameServer.Content;

namespace LeagueSandbox.GameServer.Items
{
    public class ItemManager
    {
        private Dictionary<int, ItemType> _itemTypes;

        public ItemManager()
        {
            _itemTypes = new Dictionary<int, ItemType>();
        }

        public ItemType GetItemType(int itemId)
        {
            return _itemTypes[itemId];
        }

        public ItemType SafeGetItemType(int itemId, ItemType defaultValue)
        {
            if (!_itemTypes.ContainsKey(itemId))
            {
                return defaultValue;
            }

            return _itemTypes[itemId];
        }

        public ItemType SafeGetItemType(int itemId)
        {
            return SafeGetItemType(itemId, null);
        }

        public void ResetItems()
        {
            _itemTypes.Clear();
        }

        public void AddItems(ItemContentCollection contentCollection)
        {
            foreach (var entry in contentCollection)
            {
                var itemType = ItemType.Load(this, entry.Value);
                _itemTypes.Add(entry.Key, itemType);
            }
        }
    }
}