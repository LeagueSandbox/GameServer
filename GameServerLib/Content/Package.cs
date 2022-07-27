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
using System.Linq;
using static LeagueSandbox.GameServer.Content.TalentContentCollection;

namespace LeagueSandbox.GameServer.Content
{
    public class Package
    {
        public string PackagePath { get; private set; }
        public string PackageName { get; private set; }

        private readonly Dictionary<string, CharData> _charData = new Dictionary<string, CharData>();
        private readonly Dictionary<string, SpellData> _spellData = new Dictionary<string, SpellData>();
        private readonly Dictionary<string, NavigationGrid> _navGrids = new Dictionary<string, NavigationGrid>();
        private readonly Dictionary<string, string> _mapData = new Dictionary<string, string>();
        private readonly Game _game;
        private static ILog _logger = LoggerProvider.GetLogger();

        private bool _hasScripts = false;

        public Package(string packagePath, Game game)
        {
            PackagePath = packagePath;
            _game = game;
        }

        public void LoadPackage(string packageName)
        {
            PackageName = packageName;

            LoadPackage();
            LoadScripts();
        }

        private ContentFile GetContentFileFromJson(string filePath)
        {
            ContentFile file = null;
            if (File.Exists(filePath))
            {
                var fileText = File.ReadAllText(filePath);
                file = JsonConvert.DeserializeObject<ContentFile>(fileText);
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
            if (_mapData.Count == 0)
            {
                return null;
            }

            MapData toReturnMapData = new MapData(mapId);
            JArray mapObjects;

            if (_mapData.TryGetValue("room.dsc", out string roomFile))
            {
                // Read the room file.
                var mapData = JObject.Parse(_mapData["room.dsc"]);

                // Grab the array of object reference entries.
                mapObjects = (JArray)mapData.SelectToken("entries");
            }
            else
            {
                _logger.Warn("Room file not found!");
                return null;
            }

            // Iterate through them.
            foreach (var Object in mapObjects)
            {
                string referenceName = Object.Value<string>("Name");

                MapObject objToAdd = AddMapObject(referenceName, mapId);

                // Skip empty map objects.
                if (objToAdd != MapObject.Empty)
                {
                    toReturnMapData.MapObjects.Add(referenceName, objToAdd);
                }
            }

            //Map1's Room file doesn't contain the Fountains, so we have to get that manually
            if (!toReturnMapData.MapObjects.ContainsKey("__Spawn_T1"))
            {
                for (int i = 1; i <= 2; i++)
                {
                    var mapObject = AddMapObject($"__Spawn_T{i}", mapId);
                    if (mapObject != MapObject.Empty)
                    {
                        toReturnMapData.MapObjects.Add($"__Spawn_T{i}", mapObject);
                    }
                }
            }

            // EXPCurve, DeathTimes, and StatsProgression.
            var expFileName = "ExpCurve";
            if (!string.IsNullOrEmpty(_game.Map.MapScript.MapScriptMetadata.ExpCurveOverride))
            {
                expFileName = _game.Map.MapScript.MapScriptMetadata.ExpCurveOverride;
            }

            ContentFile expFile = JsonConvert.DeserializeObject<ContentFile>(_mapData[expFileName]);
            ContentFile deathTimefile = JsonConvert.DeserializeObject<ContentFile>(_mapData["DeathTimes"]);
            ContentFile statProgressionFile = JsonConvert.DeserializeObject<ContentFile>(_mapData["StatsProgression"]);

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

            JObject serializedConstants = JsonConvert.DeserializeObject<JObject>(_mapData["Constants"]);
            foreach (JProperty childToken in serializedConstants.Children())
            {
                //TODO: Investigate if the strings in the file could be usefull for us (I doubt it)
                if (childToken.Value.Type == JTokenType.Float || childToken.Value.Type == JTokenType.Integer)
                {
                    float asdhua = childToken.Value.Value<float>();
                    toReturnMapData.MapConstants.Add(childToken.Name, asdhua);
                }
            }

            // SpawnBarracks (lane minion spawn positions)
            JObject spawnBarrack = new JObject();
            foreach (var barrackFile in _mapData.Keys.Where(x => x.Contains("Spawn_Barracks")))
            {
                spawnBarrack = JObject.Parse(_mapData[barrackFile]);

                string name = spawnBarrack.Value<string>("Name");

                var centralPoint = spawnBarrack.SelectToken("CentralPoint");
                var barrackCoords = new Vector3
                {
                    X = centralPoint.Value<float>("X"),
                    Y = centralPoint.Value<float>("Y"),
                    Z = centralPoint.Value<float>("Z")
                };

                var barrack = new MapObject(name, barrackCoords, mapId);
                toReturnMapData.SpawnBarracks.Add(name, barrack);
            }

            return toReturnMapData;
        }

        public MapObject AddMapObject(string objectName, int mapId)
        {
            if (_mapData.TryGetValue(objectName, out var mapObjectText))
            {
                // Read the object file
                var objectData = JObject.Parse(mapObjectText);

                // Grab the Name and CentralPoint
                var name = objectData.Value<string>("Name");
                var pointJson = objectData.SelectToken("CentralPoint");
                var point = new Vector3
                {
                    X = pointJson.Value<float>("X"),
                    Y = pointJson.Value<float>("Y"),
                    Z = pointJson.Value<float>("Z")
                };
                return new MapObject(name, point, mapId);
            }
            else
            {
                return MapObject.Empty;
            }
        }

        public Dictionary<string, JArray> GetMapSpawns(int mapId)
        {
            var mapName = $"Map{mapId}";
            var toReturnMapSpawns = new Dictionary<string, JArray>();
            JToken mapSpawnInformation;

            if (_mapData.TryGetValue(mapName, out string spawnsFile))
            {
                mapSpawnInformation = JObject.Parse(spawnsFile).SelectToken("spawns");
            }
            else
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
            _navGrids.TryGetValue(navgridName, out NavigationGrid navGrid);

            return navGrid;
        }

        public SpellData GetSpellData(string spellName)
        {
            if (_spellData.TryGetValue(spellName, out var spellData))
            {
                return spellData;
            }
            else
            {
                string path = $"{GetContentTypePath("Spells")}/{spellName}/{spellName}.json";
                ContentFile contentFile = GetContentFileFromJson(path);
                if (contentFile != null)
                {
                    SpellData toReturn = new SpellData().Load(contentFile);

                    _spellData.Add(spellName, toReturn);
                    return toReturn;
                }
                return null;
            }
        }

        public CharData GetCharData(string characterName)
        {
            if (_charData.TryGetValue(characterName, out var charData))
            {
                return charData;
            }
            else
            {
                string path = $"{GetContentTypePath("Stats")}/{characterName}/{characterName}.json";
                ContentFile contentFile = GetContentFileFromJson(path);
                if (contentFile != null)
                {
                    CharData toReturn = new CharData().Load(contentFile);

                    _charData.Add(characterName, toReturn);
                    return toReturn;
                }
                return null;
            }
        }

        public TalentCollectionEntry GetTalentEntry(string name)
        {
            string path = $"{GetContentTypePath("Talents")}/{name}/{name}.json";
            if (File.Exists(path))
            {
                return JsonConvert.DeserializeObject<TalentCollectionEntry>(File.ReadAllText(path));
            }
            return null;
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
            //Items should be loaded based on the Map being played, refer to "Levels/MapX/Items.inibin"
            string path = GetContentTypePath("Items");
            if (Directory.Exists(path))
            {
                foreach (var itemFile in Directory.EnumerateFiles(path, "*.json", SearchOption.AllDirectories))
                {
                    var itemData = (new ItemData()).Load(GetContentFileFromJson(itemFile));
                    _game.ItemManager.AddItemType(itemData);
                }
            }

            string mapName = $"Map{_game.Config.GameConfig.Map}";
            path = $"{GetContentTypePath("AIMesh")}/{mapName}";
            if (Directory.Exists(path))
            {
                foreach (var navGrid in Directory.EnumerateFiles(path, "*.aimesh_ngrid", SearchOption.AllDirectories))
                {
                    _navGrids.Add(Path.GetFileNameWithoutExtension(navGrid), new NavigationGrid(navGrid));
                }
            }

            path = $"{GetContentTypePath("Maps")}/{mapName}";
            if (Directory.Exists(path))
            {
                foreach (var mapFile in Directory.EnumerateFiles(path, "*.json", SearchOption.AllDirectories))
                {
                    string fileName = Path.GetFileNameWithoutExtension(mapFile);

                    if (fileName.EndsWith(".sco"))
                    {
                        fileName = fileName.Replace(".sco", "");
                    }
                    _mapData.Add(fileName, File.ReadAllText(mapFile));
                }
            }
        }

        private string GetContentTypePath(string contentType)
        {
            return $"{PackagePath}/{contentType}";
        }
    }
}
