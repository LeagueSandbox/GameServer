using System.Collections.Generic;
using System.IO;
using System.Linq;
using IniParser;
using LeagueSandbox.GameServer.Content;

namespace LeagueSandbox.GameServer.Items
{
    public class ItemManager
    {
        private Dictionary<int, ItemData> _itemData;

        public ItemManager()
        {
            _itemData = new Dictionary<int, ItemData>();
        }

        public ItemData GetItemType(int itemId)
        {
            return _itemData[itemId];
        }

        public ItemData SafeGetItemType(int itemId, ItemData defaultValue = null)
        {
            if (!_itemData.ContainsKey(itemId))
            {
                return defaultValue;
            }

            return _itemData[itemId];
        }

        public void LoadItems(ContentManager contentManager)
        {
            var iniParser = new FileIniDataParser();
            foreach (var content in contentManager.Content)
            {
                if (!content.Key.StartsWith("DATA/Items") || !content.Key.EndsWith(".ini"))
                {
                    continue;
                }

                var split = content.Key.Split('/');
                var itemIdStr = split.Last().Replace(".ini", "");

                ContentFile contentFile;
                using (var stream = new StreamReader(new MemoryStream(content.Value)))
                {
                    var iniData = iniParser.ReadData(stream);
                    contentFile = new ContentFile(ContentManager.ParseIniFile(iniData));
                }

                var itemData = ItemData.Load(this, contentFile, int.Parse(itemIdStr));
                _itemData.Add(itemData.ItemId, itemData);
            }
        }
    }
}