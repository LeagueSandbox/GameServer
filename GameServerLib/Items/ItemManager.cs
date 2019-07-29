using System.Collections.Generic;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Content;

namespace LeagueSandbox.GameServer.Items
{
    public class ItemManager
    {
        private readonly Game _game;
        private readonly Dictionary<int, IItemData> _itemTypes;

        public ItemManager(Game game)
        {
            _itemTypes = new Dictionary<int, IItemData>();
            _game = game;
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

        public void LoadItemSpelldata()
        {
            foreach (var entry in _itemTypes)
            {
                var itemData = entry.Value;
                itemData.SpellData.Load(itemData.SpellName);
            }
        }

        public void AddItems(ItemContentCollection contentCollection)
        {
            foreach (var entry in contentCollection)
            {
                var itemType = ItemData.Load(_game, entry.Value);
                _itemTypes.Add(entry.Key, itemType);
            }
        }
    }
}