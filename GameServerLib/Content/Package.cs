using System.Collections.Generic;
using System.IO;
using log4net;
using LeagueSandbox.GameServer.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using LeagueSandbox.GameServer.Content.Navigation;
using System.Numerics;
using LeagueSandbox.GameServer.Inventory;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.Handlers;
using GameServerCore.Domain;

namespace LeagueSandbox.GameServer.Content
{
    public class Package
    {
        public string PackagePath { get; private set; }
        public string PackageName { get; private set; }

        private readonly Dictionary<string, CharData> _charData = new Dictionary<string, CharData>();
        private readonly Dictionary<string, SpellData> _spellData = new Dictionary<string, SpellData>();
        private readonly Dictionary<string, ItemData> _itemData = new Dictionary<string, ItemData>();

        private readonly Game _game;
        private static ILog _logger = LoggerProvider.GetLogger();

        private readonly Dictionary<string, Dictionary<string, List<string>>> _content = new Dictionary<string, Dictionary<string, List<string>>>();

        private static readonly string[] ContentTypes = {
            "Items",
            "Maps",
            "Spells",
            "Stats",
            "Talents"
        };

        private bool _hasScripts = false;

        public Package(string packagePath, Game game)
        {
            PackagePath = packagePath;
            _game = game;
        }

        public void LoadPackage(string packageName)
        {
            PackageName = packageName;

            InitializeContent();
            LoadPackage();
            LoadTalents();
            LoadScripts();
        }

        private void InitializeContent()
        {
            foreach (var contentType in ContentTypes)
            {
                _content[contentType] = new Dictionary<string, List<string>>();
            }
        }

        public ContentFile GetContentFileFromJson(string contentType, string itemName, string subPath = null)
        {
            if (!_content.ContainsKey(contentType) || !_content[contentType].ContainsKey(itemName))
            {
                //This is just to avoid console Spamm due to it looking for content in the Scripts folder,
                //since it isn't supposed to be any content there anyway
                if (!PackageName.Contains("Scripts"))
                {
                    _logger.Debug($"Package: {PackageName} does not contain file for {itemName}.");
                }
                return null;
            }

            var fileName = $"{itemName}/{itemName}.json";

            if (subPath != null)
            {
                fileName = $"{subPath}/{itemName}.json";
            }

            var filePath = $"{GetContentTypePath(contentType)}/{fileName}";

            return GetContentFileFromJson(filePath);
        }

        private ContentFile GetContentFileFromJson(string filePath)
        {
            var file = new ContentFile();
            try
            {
                var fileText = File.ReadAllText(filePath);
                file = JsonConvert.DeserializeObject<ContentFile>(fileText);
            }
            catch (System.Exception e)
            {
                _logger.Warn(e.Message);
                return null;
            }
            return file;
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
            string mapFolder = $"{GetContentTypePath(contentType)}/{mapName}";
            string sceneDirectory = $"{mapFolder}/Scene";
            string roomFilePath = $"{sceneDirectory}/room.dsc.json";

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

            // Iterate through them.
            foreach (var Object in mapObjects)
            {
                string referenceName = Object.Value<string>("Name");

                MapObject objToAdd = AddMapObject(referenceName, contentType, mapName, mapId);

                // Skip empty map objects.
                if (objToAdd != MapObject.Empty)
                {
                    toReturnMapData.MapObjects.Add(referenceName, objToAdd);
                }
            }
            //Map1's Room file doesn't contain the Fountains, so we have to get that manually
            if (!toReturnMapData.MapObjects.ContainsKey("__Spawn_T1"))
            {
                //This is to avoid crashes when loading maps that don't have fountain files at all (Map11)
                try
                {
                    for (int i = 1; i <= 2; i++)
                    {
                        var mapObject = AddMapObject($"__Spawn_T{i}", contentType, mapName, mapId);
                        if (mapObject != MapObject.Empty)
                        {
                            toReturnMapData.MapObjects.Add($"__Spawn_T{i}", mapObject);
                        }
                    }
                }
                catch
                {
                }
            }

            // EXPCurve, DeathTimes, and StatsProgression.
            ContentFile expFile;
            ContentFile deathTimefile;
            ContentFile statProgressionFile;
            try
            {
                var expFileName = "ExpCurve";
                if (!string.IsNullOrEmpty(_game.Map.MapScript.MapScriptMetadata.ExpCurveOverride))
                {
                    expFileName = _game.Map.MapScript.MapScriptMetadata.ExpCurveOverride;
                }
                expFile = GetContentFileFromJson("Maps", expFileName, mapName);
                deathTimefile = GetContentFileFromJson("Maps", "DeathTimes", mapName);
                statProgressionFile = GetContentFileFromJson("Maps", "StatsProgression", mapName);
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

            if (expFile.Values.ContainsKey("ExpGrantedOnDeath"))
            {
                toReturnMapData.BaseExpMultiple = expFile.GetFloat("ExpGrantedOnDeath", "BaseExpMultiple", 0);
                toReturnMapData.LevelDifferenceExpMultiple = expFile.GetFloat("ExpGrantedOnDeath", "LevelDifferenceExpMultiple", 0);
                toReturnMapData.MinimumExpMultiple = expFile.GetFloat("ExpGrantedOnDeath", "MinimumExpMultiple", 0);
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

            //Map Constants
            if (File.Exists($"{mapFolder}/Constants.json"))
            {
                string constantsText = File.ReadAllText($"{mapFolder}/Constants.json");
                JObject serializedConstants = JsonConvert.DeserializeObject<JObject>(constantsText);

                foreach (JProperty childToken in serializedConstants.Children())
                {
                    //TODO: Investigate if the strings in the file could be usefull for us (I doubt it)
                    if (childToken.Value.Type == JTokenType.Float || childToken.Value.Type == JTokenType.Integer)
                    {
                        float asdhua = childToken.Value.Value<float>();
                        toReturnMapData.MapConstants.Add(childToken.Name, asdhua);
                    }
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

                    var barracks = new MapObject(name, barrackCoords, mapId);

                    toReturnMapData.SpawnBarracks.Add(name, barracks);
                }
            }
            return toReturnMapData;
        }

        public MapObject AddMapObject(string objectName, string contentType, string mapName, int mapId)
        {
            // Define the full path to the object file.
            var objectFileName = $"{mapName}/Scene/{objectName}.sco";
            var objectFilePath = $"{GetContentTypePath(contentType)}/{objectFileName}.json";

            // Create empty mapObject so we can fill it after we successfully read the object file.
            MapObject mapObject;

            try
            {
                var tryFiles = Directory.GetFiles(GetContentTypePath(contentType), $"{objectFileName}.json", new EnumerationOptions { MatchCasing = MatchCasing.CaseInsensitive });

                if (tryFiles.Length > 0)
                {
                    objectFilePath = tryFiles[0];

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
                    mapObject = new MapObject(name, point, mapId);
                }
                else
                {
                    return MapObject.Empty;
                }
            }
            catch (JsonReaderException)
            {
                return MapObject.Empty;
            }

            // Add the reference name and filled map object.
            return mapObject;
        }

        public Dictionary<string, JArray> GetMapSpawns(int mapId)
        {
            var mapName = $"Map{mapId}";
            var contentType = "Maps";
            var toReturnMapSpawns = new Dictionary<string, JArray>();

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
                toReturnMapSpawns.Add(team, spawnsByPlayerCount);
            }

            return toReturnMapSpawns;
        }

        public NavigationGrid GetNavigationGrid(MapScriptHandler map)
        {
            string navgridName = "AIPath";
            if (!string.IsNullOrEmpty(map.MapScript.MapScriptMetadata.NavGridOverride))
            {
                navgridName = map.MapScript.MapScriptMetadata.NavGridOverride;
            }

            string navigationGridPath = $"{this.PackagePath}/AIMesh/Map{map.Id}/{navgridName}.aimesh_ngrid";

            if (!File.Exists(navigationGridPath))
            {
                _logger.Debug($"{this.PackageName} does not contain a navgrid, skipping...");
                return null;
            }

            return new NavigationGrid(navigationGridPath);
        }

        public SpellData GetSpellData(string spellName)
        {
            return _spellData.GetValueOrDefault(spellName, null);
        }

        public CharData GetCharData(string characterName)
        {
            return _charData.GetValueOrDefault(characterName, null);
        }

        public bool HasScripts()
        {
            return _hasScripts;
        }

        public bool LoadScripts()
        {
            var scriptLoadResult = _game.ScriptEngine.LoadSubDirectoryScripts(PackagePath);
            switch (scriptLoadResult)
            {
                case CompilationStatus.Compiled:
                    {
                        _logger.Debug($"Loaded all C# scripts from package: {PackageName}");
                        _hasScripts = true;
                        return true;
                    }
                case CompilationStatus.SomeCompiled:
                    {
                        _logger.Debug($"Loaded some C# scripts from package: {PackageName}");
                        _hasScripts = true;
                        return true;
                    }
                case CompilationStatus.NoneCompiled:
                    {
                        _logger.Debug($"{PackageName} failed to compile all C# scripts...");
                        _hasScripts = true;
                        return false;
                    }
                case CompilationStatus.NoScripts:
                    {
                        _logger.Debug($"{PackageName} does not have C# scripts, skipping...");
                        _hasScripts = false;
                        return true;
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

                var contentTypePath = GetContentTypePath(contentType);

                if (!Directory.Exists(contentTypePath))
                {
                    _logger.Debug($"Package {PackageName} does not contain {contentType}, skipping...");
                    continue;
                }

                var fileList = Directory.EnumerateFiles(contentTypePath, "*.json", SearchOption.AllDirectories);

                foreach (var filePath in fileList)
                {
                    if (contentType == "Stats" || contentType == "Spells" || contentType == "Items")
                    {
                        var file = GetContentFileFromJson(filePath);
                        if (file != null)
                        {
                            var name = file.Name;

                            switch (contentType)
                            {
                                case "Stats":
                                    {
                                        _charData[name] = (new CharData()).Load(file);
                                        break;
                                    }

                                case "Spells":
                                    {
                                        _spellData[name] = (new SpellData()).Load(file);
                                        break;
                                    }

                                case "Items":
                                    {
                                        var itemData = (new ItemData()).Load(file);
                                        _game.ItemManager.AddItemType(itemData);
                                        break;
                                    }
                            }
                        }
                    }
                    else
                    {
                        var fileName = Path.GetFileNameWithoutExtension(filePath);
                        if (fileName != null)
                        {
                            _content[contentType][fileName] = new List<string> { PackageName };
                        }
                    }
                }
            }
        }

        private void LoadTalents()
        {
            try
            {
                TalentContentCollection.LoadMasteriesFrom($"{PackagePath}/Talents");
            }
            catch (DirectoryNotFoundException)
            {
                _logger.Debug($"Package: {PackageName} does not contain any Talents, skipping...");
            }
        }

        private string GetContentTypePath(string contentType)
        {
            return $"{PackagePath}/{contentType}";
        }
    }
}
