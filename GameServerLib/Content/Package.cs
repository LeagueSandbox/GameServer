using System.Collections.Generic;
using System.IO;
using GameServerCore.Content;
using GameServerCore.Domain;
using log4net;
using LeagueSandbox.GameServer.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LeagueSandbox.GameServer.Content.Navigation;
using GameServerCore.Domain.GameObjects.Spell;
using System.Numerics;

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

        public IContentFile GetContentFileFromJson(string contentType, string itemName, string subPath = null)
        {
            if (!_content.ContainsKey(contentType) || !_content[contentType].ContainsKey(itemName))
            {
                _logger.Debug($"Package: {PackageName} does not contain file for {itemName}.");
                return null;
            }

            var fileName = $"{itemName}/{itemName}.json";

            if (subPath != null)
            {
                fileName = $"{subPath}/{itemName}.json";
            }

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

        /// <summary>
        /// Reads through the room file of the given map to create MapData which includes all of the map's objects.
        /// </summary>
        /// <param name="mapId">Map to read.</param>
        /// <returns>MapData containing all objects listed in the room file.</returns>
        public MapData GetMapData(int mapId)
        {
            // Define the end of the file path and setup return variable.
            var mapName = $"Map{mapId}";
            var contentType = "Maps";
            var toReturnMapData = new MapData(mapId);

            // Verify that the content exists.
            if (!_content.ContainsKey(contentType) || !_content[contentType].ContainsKey(mapName))
            {
                return null;
            }

            // MapObjects

            // Define the full path to the room file which houses references to all objects.
            var sceneDirectory = $"{GetContentTypePath(contentType)}/{mapName}/Scene";
            var roomFilePath = $"{sceneDirectory}/room.dsc.json";

            // Declare empty room variable.
            JArray mapObjects;

            // To prevent crashes if the files are missing.
            try
            {
                // Read the room file.
                var mapData = JObject.Parse(File.ReadAllText(roomFilePath));

                // Grab the array of object reference entries.
                mapObjects = (JArray)mapData.SelectToken("entries");
            }
            // If failed to read.
            catch (JsonReaderException)
            {
                return null;
            }

            // Get the number of objects in the entries array.
            var objectCount = mapObjects.Count;
            // Iterate through them.
            for (var i = 0; i < objectCount; i++)
            {
                // Get the object reference name.
                string nameReference = mapObjects[i].Value<string>("Name");

                // Define the full path to the object file.
                var objectFileName = $"{mapName}/Scene/{nameReference}.sco";
                var objectFilePath = $"{GetContentTypePath(contentType)}/{objectFileName}.json";

                // Create empty mapObject so we can fill it after we successfully read the object file.
                MapData.MapObject mapObject;

                try
                {
                    // Read the object file
                    var objectData = JObject.Parse(File.ReadAllText(objectFilePath));

                    // Grab the Name and CentralPoint
                    var name = objectData.Value<string>("Name");
                    var pointJson = objectData.SelectToken("CentralPoint");
                    var point = new Vector3
                    {
                        X = pointJson.Value<float>("X"),
                        Y = pointJson.Value<float>("Y"),
                        Z = pointJson.Value<float>("Z")
                    };

                    mapObject = new MapData.MapObject(name, point, mapId);
                }
                catch (JsonReaderException)
                {
                    continue;
                }

                // Add the reference name and filled map object.
                toReturnMapData.MapObjects.Add(nameReference, mapObject);
            }

            // EXPCurve, DeathTimes, and StatsProgression.

            var expFile = new ContentFile();
            var deathTimefile = new ContentFile();
            var statProgressionFile = new ContentFile();
            try
            {
                expFile = (ContentFile)GetContentFileFromJson("Maps", "ExpCurve", mapName);
                deathTimefile = (ContentFile)GetContentFileFromJson("Maps", "DeathTimes", mapName);
                statProgressionFile = (ContentFile)GetContentFileFromJson("Maps", "StatsProgression", mapName);
            }
            catch (ContentNotFoundException exception)
            {
                _logger.Warn(exception.Message);
                return null;
            }

            if (expFile.Values.ContainsKey("EXP"))
            {
                // We skip the first level, meaning there are 29 level instances, but we only assign 2->29 (that's 29).
                // To fix this (assign 2->30), we add 1 to the Count.
                for (int i = 2; i <= expFile.Values["EXP"].Count + 1; i++)
                {
                    toReturnMapData.ExpCurve.Add(expFile.GetFloat("EXP", $"Level{i}"));
                }
            }

            if (deathTimefile.Values.ContainsKey("TimeDeadPerLevel"))
            {
                for (int i = 1; i < deathTimefile.Values["TimeDeadPerLevel"].Count; i++)
                {
                    if (i <= 9)
                    {
                        toReturnMapData.DeathTimes.Add(deathTimefile.GetFloat("TimeDeadPerLevel", $"Level0{i}"));
                    }
                    else
                    {
                        toReturnMapData.DeathTimes.Add(deathTimefile.GetFloat("TimeDeadPerLevel", $"Level{i}"));
                    }
                }
            }

            if (statProgressionFile.Values.ContainsKey("PerLevelStatsFactor"))
            {
                for (int i = 0; i < statProgressionFile.Values["PerLevelStatsFactor"].Count; i++)
                {
                    toReturnMapData.StatsProgression.Add(statProgressionFile.GetFloat("PerLevelStatsFactor", $"Level{i}"));
                }
            }

            // SpawnBarracks (lane minion spawn positions)

            JObject spawnBarracks = new JObject();
            foreach (var file in Directory.GetFiles(sceneDirectory))
            {
                if (file.Contains("Spawn_Barracks"))
                {
                    var barrack = Path.GetFileName(file);
                    var path = $"{sceneDirectory}/{barrack}";
                    try
                    {
                        spawnBarracks = JObject.Parse(File.ReadAllText(path));
                    }
                    catch (ContentNotFoundException exception)
                    {
                        _logger.Warn(exception.Message);
                        return null;
                    }

                    string name = spawnBarracks.Value<string>("Name");

                    var centralPoint = spawnBarracks.SelectToken("CentralPoint");
                    var barrackCoords = new Vector3
                    {
                        X = centralPoint.Value<float>("X"),
                        Y = centralPoint.Value<float>("Y"),
                        Z = centralPoint.Value<float>("Z")
                    };

                    var barracks = new MapData.MapObject(name, barrackCoords, mapId);

                    toReturnMapData.SpawnBarracks.Add(name, barracks);
                }
            }

            return toReturnMapData;
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
            var scriptLoadResult = _game.ScriptEngine.LoadSubDirectoryScripts(PackagePath);
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
                        _logger.Debug($"{PackageName} failed to compile all C# scripts...");
                        return false;
                    }
                case Scripting.CSharp.CompilationStatus.NoScripts:
                    {
                        _logger.Debug($"{PackageName} does not have C# scripts, skipping...");
                        return false;
                    }
                default:
                    {
                        return false;
                    }
            }
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

                _content[contentType][fileName] = new List<string> { PackageName };
            }
        }

        private string GetContentTypePath(string contentType)
        {
            return $"{PackagePath}/{contentType}";
        }
    }
}
