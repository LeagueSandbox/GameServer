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
        public string ResourcePath { get { return Convert.ToString(MetaData["ResourcePath"]); } }
        public string Name { get { return Convert.ToString(MetaData["Name"]); } }
        public object Id { get { return MetaData["Id"]; } }

        public T GetValue<T>(string section, string name, T defaultValue)
        {
            try
            {
                return (T)Convert.ChangeType(Values[section][name], typeof(T));
            }
            catch (FormatException)
            {
                return defaultValue;
            }
        }

        public T SafeGetValue<T>(string section, string name, T defaultValue)
        {
            if (!Values.ContainsKey(section) || !Values[section].ContainsKey(name))
            {
                return defaultValue;
            }

            return GetValue(section, name, defaultValue);
        }

        public float SafeGetFloat(string section, string name, float defaultValue = 0f)
        {
            return float.Parse(SafeGetValue(section, name, defaultValue).ToString());
        }

        public int SafeGetInt(string section, string name, int defaultValue = 0)
        {
            return SafeGetValue(section, name, defaultValue);
        }

        public string SafeGetString(string section, string name, string defaultValue = "")
        {
            return SafeGetValue(section, name, defaultValue);
        }

        public bool SafeGetBool(string section, string name, bool defaultValue = false)
        {
            return SafeGetValue(section, name, defaultValue);
        }
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
