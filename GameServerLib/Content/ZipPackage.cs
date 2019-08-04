using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServerCore.Content;
using GameServerCore.Domain;
using log4net;
using LeagueSandbox.GameServer.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.GameServer.Content
{
    public class ZipPackage : IPackage
    {
        public string PackageName { get; private set; }
        public string PackagePath { get; private set; }

        private readonly Dictionary<string, ISpellData> _spellData = new Dictionary<string, ISpellData>();
        private readonly Dictionary<string, ICharData> _charData = new Dictionary<string, ICharData>();

        private readonly Game _game;
        private readonly ILog _logger;

        private readonly Dictionary<string, Dictionary<string, List<string>>> _content = new Dictionary<string, Dictionary<string, List<string>>>();

        private static readonly string[] ContentTypes = {
            "Champions",
            "Items",
            "Buffs",
            "Maps",
            "Spells",
            "Stats"
        };

        public ZipPackage(string zipPackagePath, Game game)
        {
            PackagePath = zipPackagePath;

            _game = game;
            _logger = LoggerProvider.GetLogger();
        }

        public void LoadPackage(string packageName)
        {
            PackageName = packageName;

            InitializeContent();
            LoadPackage();
            LoadItems();
            LoadScripts();
        }

        private void InitializeContent()
        {
            foreach (var contentType in ContentTypes)
            {
                _content[contentType] = new Dictionary<string, List<string>>();
            }
        }

        private void LoadPackage()
        {
            foreach (var contentType in ContentTypes)
            {
                LoadData(contentType);
            }
        }

        private void LoadData(string contentType)
        {
            if (!File.Exists(PackagePath))
            {
                return;
            }

            var contentTypeFolder = $"{PackageName}/{contentType}/";

            using (var archive = ZipFile.OpenRead(PackagePath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!entry.FullName.StartsWith(contentTypeFolder) || entry.FullName.Equals(contentTypeFolder))
                    {
                        continue;
                    }

                    if (entry.FullName.EndsWith(".json"))
                    {
                        var itemName = Path.GetFileNameWithoutExtension(entry.FullName);
                        _content[contentType][itemName] = new List<string> { PackageName };
                    }
                }
            }
        }

        private void LoadItems()
        {
            try
            {
                _game.ItemManager.AddItems(ItemContentCollection.LoadItemsFromZip(PackagePath, PackageName));
            }
            catch (DirectoryNotFoundException)
            {
                _logger.Debug($"Package: {PackageName} does not contain any items, skipping...");
            }
        }

        public bool LoadScripts()
        {
            var scriptLoadResult = _game.ScriptEngine.LoadSubdirectoryScriptsZip(PackagePath);

            if (scriptLoadResult)
            {
                _logger.Debug($"Loaded C# scripts from package: {PackageName}");
                return true;
            }

            _logger.Debug($"{PackageName} does not contain C# scripts, skipping...");
            return false;
        }

        public IContentFile GetContentFileFromJson(string contentType, string itemName)
        {
            if (!_content.ContainsKey(contentType) || !_content[contentType].ContainsKey(itemName))
            {
                _logger.Debug($"Package: {PackageName} does not contain file for {itemName}.");
                return null;
            }

            var filePath = $"{PackageName}/{contentType}/{itemName}/{itemName}.json";

            IContentFile toReturnContentFile;

            using (var archive = ZipFile.OpenRead(PackagePath))
            {
                var file = archive.GetEntry(filePath);

                if (file == null)
                {
                    return null;
                }

                var fileText = new StreamReader(file.Open(), Encoding.Default).ReadToEnd();
                toReturnContentFile = JsonConvert.DeserializeObject<ContentFile>(fileText);
            }

            return toReturnContentFile;
        }

        public IMapSpawns GetMapSpawns(int mapId)
        {
            const string contentType = "Maps";
            var mapName = $"Map{mapId}";
            var filePath = $"{PackageName}/{contentType}/{mapName}/{mapName}.json";
            var toReturnMapSpawns = new MapSpawns();

            if (!_content.ContainsKey(contentType) || !_content[contentType].ContainsKey(mapName))
            {
                _logger.Debug($"Package: {PackageName} does not contain file for {mapName}.");
                return null;
            }

            JToken mapSpawnInformation;

            using (var archive = ZipFile.OpenRead(PackagePath))
            {
                var file = archive.GetEntry(filePath);

                if (file == null)
                {
                    return null;
                }

                var fileText = new StreamReader(file.Open(), Encoding.Default).ReadToEnd();

                try
                {
                    var mapData = JObject.Parse(fileText);

                    mapSpawnInformation = mapData.SelectToken("spawns");
                }
                catch (JsonReaderException)
                {
                    return null;
                }
            }

            foreach (JProperty teamSpawn in mapSpawnInformation)
            {
                var team = teamSpawn.Name;
                var spawnsByPlayerCount = (JArray)teamSpawn.Value;
                for (var i = 0; i < spawnsByPlayerCount.Count; i++)
                {
                    var playerSpawns = new PlayerSpawns((JArray)spawnsByPlayerCount[i]);
                    toReturnMapSpawns.SetSpawns(team, playerSpawns, i);
                }
            }

            return toReturnMapSpawns;
        }

        public INavGrid GetNavGrid(int mapId)
        {
            var navgridPath = $"{PackageName}/AIMesh/Map{mapId}/AIPath.aimesh_ngrid";

            string fileText;

            using (var archive = ZipFile.OpenRead(PackagePath))
            {
                var file = archive.GetEntry(navgridPath);

                if (file == null)
                {
                    return null;
                }

                fileText = new StreamReader(file.Open(), Encoding.Default).ReadToEnd();
            }

            return NavGridReader.ReadBinary(Encoding.Default.GetBytes(fileText));
        }

        public ISpellData GetSpellData(string spellName)
        {
            if (_spellData.ContainsKey(spellName))
            {
                return _spellData[spellName];
            }

            _spellData[spellName] = new SpellData(_game.Config.ContentManager);
            _spellData[spellName].Load(spellName);
            return _spellData[spellName];
        }

        public ICharData GetCharData(string characterName)
        {
            if (_charData.ContainsKey(characterName))
            {
                return _charData[characterName];
            }
            _charData[characterName] = new CharData(_game.Config.ContentManager);
            _charData[characterName].Load(characterName);
            return _charData[characterName];
        }
    }
}
