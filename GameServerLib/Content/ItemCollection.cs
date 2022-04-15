using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeagueSandbox.GameServer.Logging;
using log4net;
using Newtonsoft.Json;

namespace LeagueSandbox.GameServer.Content
{
    public class ContentCollectionEntry : ContentFile
    {
        public int ContentFormatVersion => Convert.ToInt32(MetaData["ContentFormatVersion"]);
        public string ResourcePath => Convert.ToString(MetaData["ResourcePath"]);
        public string Name => Convert.ToString(MetaData["Name"]);
        public object Id => MetaData["Id"];
    }

    public class ContentCollection
    {
    }

    public class ItemContentCollectionEntry : ContentCollectionEntry
    {
        public int ItemId => Convert.ToInt32(Id);
    }

    public class ItemContentCollection : ContentCollection
    {
        private Dictionary<int, ItemContentCollectionEntry> _items;
        ILog _logger = LoggerProvider.GetLogger();

        public Dictionary<int, ItemContentCollectionEntry>.Enumerator GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        private ItemContentCollection()
        {
            _items = new Dictionary<int, ItemContentCollectionEntry>();
        }

        private void AddFromPath(string dataPath)
        {
            string data;
            //This is so it doesn't try to load items from the scripts folders
            try
            {
                data = File.ReadAllText(dataPath);
            }
            catch
            {
                _logger.Debug($"File not found in {dataPath}. Skipping...");
                return;
            }
            var collectionEntry = JsonConvert.DeserializeObject<ItemContentCollectionEntry>(data);
            _items.Add(collectionEntry.ItemId, collectionEntry);
        }

        public static ItemContentCollection LoadItemsFrom(string directoryPath)
        {
            var result = new ItemContentCollection();
            var files = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                result.AddFromPath(file);
            }

            return result;
        }
    }
}
