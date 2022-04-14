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

        public static Dictionary<string, TalentCollectionEntry> _talents { get; private set; } = new Dictionary<string, TalentCollectionEntry>();

        public static void LoadMasteriesFrom(string directoryPath)
        {
            var files = Directory.GetFiles(directoryPath, "*.json", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string data = File.ReadAllText(file);
                var collectionEntry = JsonConvert.DeserializeObject<TalentCollectionEntry>(data);
                _talents.Add(collectionEntry.Name, collectionEntry);
            }
        }

        public static bool TalentIsValid(string talent)
        {
            return _talents.ContainsKey(talent);
        }

        public static byte GetTalentMaxRank(string mastery)
        {
            return _talents[mastery].MaxLevel;
        }
    }
}
