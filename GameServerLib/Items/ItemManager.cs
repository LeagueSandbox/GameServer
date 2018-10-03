using System.Collections.Generic;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Content;

namespace LeagueSandbox.GameServer.Items
{
    public class ItemManager
    {
        private readonly Dictionary<int, IItemType> _itemTypes;

        public ItemManager()
        {
            _itemTypes = new Dictionary<int, IItemType>();
        }

        public IItemType GetItemType(int itemId)
        {
            return _itemTypes[itemId];
        }

        public IItemType SafeGetItemType(int itemId, IItemType defaultValue)
        {
            if (!_itemTypes.ContainsKey(itemId))
            {
                return defaultValue;
            }

            return _itemTypes[itemId];
        }

        public IItemType SafeGetItemType(int itemId)
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