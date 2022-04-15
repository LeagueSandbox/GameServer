using System.Collections.Generic;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Content;

namespace LeagueSandbox.GameServer.Inventory
{
    public class ItemManager
    {
        private readonly Dictionary<int, IItemData> _itemTypes;

        public ItemManager()
        {
            _itemTypes = new Dictionary<int, IItemData>();
        }

        public IItemData GetItemType(int itemId)
        {
            return _itemTypes[itemId];
        }

        public IItemData SafeGetItemType(int itemId, IItemData defaultValue)
        {
            if (!_itemTypes.ContainsKey(itemId))
            {
                return defaultValue;
            }

            return _itemTypes[itemId];
        }

        public IItemData SafeGetItemType(int itemId)
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
                var itemType = ItemData.Load(this, entry.Value);
                _itemTypes.Add(entry.Key, itemType);
            }
        }
    }
}