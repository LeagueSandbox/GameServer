using LeagueSandbox.GameServer.Core.Logic;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Content
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
            if (!Values.ContainsKey(section)) return defaultValue;
            if (!Values[section].ContainsKey(name)) return defaultValue;
            return GetValue<T>(section, name);
        }

        public float SafeGetFloat(string section, string name, float defaultValue)
        {
            return SafeGetValue(section, name, defaultValue);
        }

        public float SafeGetFloat(string section, string name)
        {
            return SafeGetFloat(section, name, 0f);
        }

        public int SafeGetInt(string section, string name, int defaultValue)
        {
            return SafeGetValue(section, name, defaultValue);
        }

        public int SafeGetInt(string section, string name)
        {
            return SafeGetInt(section, name, 0);
        }

        public string SafeGetString(string section, string name, string defaultValue)
        {
            return SafeGetValue(section, name, defaultValue);
        }

        public string SafeGetString(string section, string name)
        {
            return SafeGetString(section, name, "");
        }

        public bool SafeGetBool(string section, string name, bool defaultValue)
        {
            return SafeGetValue(section, name, defaultValue);
        }

        public bool SafeGetBool(string section, string name)
        {
            return SafeGetBool(section, name, false);
        }
    }

    public class ContentCollection
    {
    }

    public class ItemCollectionEntry : ContentCollectionEntry
    {
        public string ItemFileName { get { return Convert.ToString(MetaData["ItemFileName"]); } }
        public string ItemName { get { return Convert.ToString(MetaData["ItemName"]); } }
        public int ItemId { get { return Convert.ToInt32(MetaData["ItemId"]); } }
    }

    public class ItemCollection : ContentCollection
    {
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
