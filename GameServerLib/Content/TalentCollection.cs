using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace LeagueSandbox.GameServer.Content
{

    public class TalentContentCollection
    {
        public class TalentCollectionEntry : ContentFile
        {
            public byte MaxLevel => Convert.ToByte(Values["SpellData"]["Ranks"]);
            public string Name => Convert.ToString(MetaData["Name"]);
            public object Id => MetaData["Id"];
        }

        private static Dictionary<string, TalentCollectionEntry> _masteries = new Dictionary<string, TalentCollectionEntry>();
        public static void LoadMasteriesFrom(string directoryPath)
        {
            var files = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string data = File.ReadAllText(file);
                var collectionEntry = JsonConvert.DeserializeObject<TalentCollectionEntry>(data);
                _masteries.Add(collectionEntry.Name, collectionEntry);
            }
        }

        public static byte GetMasteryMaxLevel(string mastery)
        {
            return _masteries[mastery].MaxLevel;
        }
    }
}
