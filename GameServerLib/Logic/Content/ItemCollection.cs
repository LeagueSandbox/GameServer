using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LeagueSandbox.GameServer.Logic.Content
{
    public class ContentCollectionEntry : ContentFile
    {
        public int ContentFormatVersion { get { return Convert.ToInt32(MetaData["ContentFormatVersion"]); } }
        public string ResourcePath { get { return Convert.ToString(MetaData["ResourcePath"]); } }
        public string Name { get { return Convert.ToString(MetaData["Name"]); } }
        public object Id { get { return MetaData["Id"]; } }
    }

    public class ContentCollection
    {
    }

    public class ItemContentCollectionEntry : ContentCollectionEntry
    {
        public int ItemId { get { return Convert.ToInt32(Id); } }
    }

    public class ItemContentCollection : ContentCollection
    {
        private Dictionary<int, ItemContentCollectionEntry> _items;
        public Dictionary<int, ItemContentCollectionEntry>.Enumerator GetEnumerator() { return _items.GetEnumerator(); }

        private ItemContentCollection()
        {
            _items = new Dictionary<int, ItemContentCollectionEntry>();
        }

        private void AddFromPath(string dataPath)
        {
            var data = File.ReadAllText(dataPath);
            var collectionEntry = JsonConvert.DeserializeObject<ItemContentCollectionEntry>(data);
            _items.Add(collectionEntry.ItemId, collectionEntry);
        }

        public static ItemContentCollection LoadItemsFrom(string directoryPath)
        {
            var result = new ItemContentCollection();
            var itemDirectoryPaths = Directory.GetDirectories(directoryPath);
            foreach(var location in itemDirectoryPaths)
            {
                var path = location.Replace('\\', '/');
                var itemName = path.Split('/').Last();
                var itemDataPath = string.Format("{0}/{1}.json", path, itemName);
                result.AddFromPath(itemDataPath);
            }
            return result;
        }
    }
}
