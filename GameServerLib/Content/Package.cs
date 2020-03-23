using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameServerCore.Content;
using GameServerCore.Domain;
using log4net;
using log4net.Repository.Hierarchy;
using LeagueSandbox.GameServer.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LeagueSandbox.GameServer.Content.Navigation;

namespace LeagueSandbox.GameServer.Content
{
    public class Package : IPackage
    {
        public string PackagePath { get; private set; }
        public string PackageName { get; private set; }

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

        public Package(string packagePath, Game game)
        {
            PackagePath = packagePath;
            
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

        public IContentFile GetContentFileFromJson(string contentType, string itemName)
        {
            if (!_content.ContainsKey(contentType) || !_content[contentType].ContainsKey(itemName))
            {
                _logger.Debug($"Package: {PackageName} does not contain file for {itemName}.");
                return null;
            }

            var fileName = $"{itemName}/{itemName}.json";
            var filePath = $"{GetContentTypePath(contentType)}/{fileName}";
            var fileText = File.ReadAllText(filePath);

            IContentFile toReturnContentFile;

            try
            {
                toReturnContentFile = JsonConvert.DeserializeObject<ContentFile>(fileText);
            }
            catch (JsonSerializationException)
            {
                return null;
            }

            return toReturnContentFile;
        }

        public MapSpawns GetMapSpawns(int mapId)
        {
            var mapName = $"Map{mapId}";
            var contentType = "Maps";
            var toReturnMapSpawns = new MapSpawns();


            if (!_content.ContainsKey(contentType) || !_content[contentType].ContainsKey(mapName))
            {
                return null;
            }

            var fileName = $"{mapName}/{mapName}";
            var filePath = $"{GetContentTypePath(contentType)}/{fileName}.json";

            JToken mapSpawnInformation;

            try
            {
                var mapData = JObject.Parse(File.ReadAllText(filePath));

                mapSpawnInformation = mapData.SelectToken("spawns");
            }
            catch (JsonReaderException)
            {
                return null;
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

        public INavigationGrid GetNavigationGrid(int mapId)
        {
            string navigationGridPath = $"{this.PackagePath}/AIMesh/Map{mapId}/AIPath.aimesh_ngrid";

            if (!File.Exists(navigationGridPath))
            {
                this._logger.Debug($"{this.PackageName} does not contain a navgrid, skipping...");
                return null;
            }

            return new NavigationGrid(navigationGridPath);
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

        public bool LoadScripts()
        {
            var scriptLoadResult = _game.ScriptEngine.LoadSubdirectoryScripts(PackagePath);
            switch (scriptLoadResult)
            {
                case Scripting.CSharp.CompilationStatus.Compiled:
                    {
                        _logger.Debug($"Loaded all C# scripts from package: {PackageName}");
                        return true;
                    }
                case Scripting.CSharp.CompilationStatus.SomeCompiled:
                    {
                        _logger.Debug($"Loaded some C# scripts from package: {PackageName}");
                        return true;
                    }
                case Scripting.CSharp.CompilationStatus.NoneCompiled:
                    {
                        _logger.Debug($"{PackageName} failed to compile C# scripts...");
                        return false;
                    }
                case Scripting.CSharp.CompilationStatus.NoScripts:
                    {
                        _logger.Debug($"{PackageName} does not contain C# scripts, skipping...");
                        return false;
                    }
            }
            return false;
        }

        private void LoadPackage()
        {
            foreach (var contentType in ContentTypes)
            {
                LoadData(contentType);
            }
        }

        private void LoadItems()
        {
            try
            {
                _game.ItemManager.AddItems(ItemContentCollection.LoadItemsFrom($"{PackagePath}/Items"));
            }
            catch (DirectoryNotFoundException)
            {
                _logger.Debug($"Package: {PackageName} does not contain any items, skipping...");
            }
        }

        private void LoadData(string contentType)
        {
            var contentTypePath = GetContentTypePath(contentType);

            if (!Directory.Exists(contentTypePath))
            {
                _logger.Debug($"Package {PackageName} does not contain {contentType}, skipping...");
                return;
            }

            var fileList = Directory.GetFiles(contentTypePath, "*.json", SearchOption.AllDirectories);

            foreach (var jsonFile in fileList)
            {
                var fileName = Path.GetFileNameWithoutExtension(jsonFile);

                if (fileName == null)
                {
                    continue;
                }

                _content[contentType][fileName] = new List<string> {PackageName};
            }
        }

        private string GetContentTypePath(string contentType)
        {
            return $"{PackagePath}/{contentType}";
        }
    }
}
