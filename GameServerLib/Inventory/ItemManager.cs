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

        public IItemData SafeGetItemType(int itemId)
        {
            return _itemTypes.GetValueOrDefault(itemId, null);
        }

        public void ResetItems()
        {
            _itemTypes.Clear();
        }

        public void AddItemType(ItemData itemType)
        {
            itemType.CreateRecipe(this);
            _itemTypes.Add(itemType.ItemId, itemType);
        }
    }
}