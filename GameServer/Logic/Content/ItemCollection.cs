using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Content
{
    public class ContentCollection
    {
        public class ContentCollectionEntry
        {
            public Dictionary<string, Dictionary<string, object>> Values { get; set; }
            public Dictionary<string, object> MetaData { get; set; }

            public int ContentFormatVersion { get { return Convert.ToInt32(MetaData["ContentFormatVersion"]); } }

            public T GetValue<T>(string section, string name)
            {
                return (T)Convert.ChangeType(Values[section][name], typeof(T));
            }

            public T SafeGetValue<T>(string section, string name, T defaultValue)
            {
                if (!Values.ContainsKey(section)) return LogSkipAndReturn(section, name, defaultValue);
                if (!Values[section].ContainsKey(name)) return LogSkipAndReturn(section, name, defaultValue);
                return GetValue<T>(section, name);
            }

            private T LogSkipAndReturn<T>(string section, string name, T value)
            {
                return value;
            }
        }
    }

    public class ItemCollection : ContentCollection
    {
        public class ItemCollectionEntry : ContentCollectionEntry
        {
            public string ItemFileName { get { return Convert.ToString(MetaData["ItemFileName"]); } }
            public string ItemName { get { return Convert.ToString(MetaData["ItemName"]); } }
            public int ItemId { get { return Convert.ToInt32(MetaData["ItemId"]); } }
        }

        private Dictionary<int, ItemCollectionEntry> _items;
        public Dictionary<int, ItemCollectionEntry>.Enumerator GetEnumerator() { return _items.GetEnumerator(); }

        private ItemCollection()
        {
            _items = new Dictionary<int, ItemCollectionEntry>();
        }

        private void AddFromPath(string dataPath)
        {
            var data = File.ReadAllText(dataPath);
            var collectionEntry = JsonConvert.DeserializeObject<ItemCollectionEntry>(data);
            _items.Add(collectionEntry.ItemId, collectionEntry);
        }

        public static ItemCollection LoadItemsFrom(string directoryPath)
        {
            var result = new ItemCollection();
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
