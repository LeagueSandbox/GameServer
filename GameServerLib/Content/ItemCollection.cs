using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
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
            var data = File.ReadAllText(dataPath);
            var collectionEntry = JsonConvert.DeserializeObject<ItemContentCollectionEntry>(data);
            _items.Add(collectionEntry.ItemId, collectionEntry);
        }

        private void AddFromPathZip(string dataFile)
        {
            var collectionEntry = JsonConvert.DeserializeObject<ItemContentCollectionEntry>(dataFile);
            _items.Add(collectionEntry.ItemId, collectionEntry);
        }

        public static ItemContentCollection LoadItemsFrom(string directoryPath)
        {
            var result = new ItemContentCollection();
            var itemDirectoryPaths = Directory.GetDirectories(directoryPath);
            foreach (var location in itemDirectoryPaths)
            {
                var path = location.Replace('\\', '/');
                var itemName = path.Split('/').Last();
                var itemDataPath = $"{path}/{itemName}.json";
                result.AddFromPath(itemDataPath);
            }

            return result;
        }

        public static ItemContentCollection LoadItemsFromZip(string zipLocation, string packageName)
        {
            var result = new ItemContentCollection();

            var contentTypeFolder = $"{packageName}/Items/";

            using (var archive = ZipFile.OpenRead(zipLocation))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!entry.FullName.StartsWith(contentTypeFolder) || entry.FullName.Equals(contentTypeFolder))
                    {
                        continue;
                    }

                    if (entry.FullName.EndsWith(".json"))
                    {
                        var dataFile = new StreamReader(entry.Open(), Encoding.Default).ReadToEnd();
                        result.AddFromPathZip(dataFile);
                    }
                }
            }

            return result;
        }
    }
}
