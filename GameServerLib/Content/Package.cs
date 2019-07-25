﻿using System;
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

namespace LeagueSandbox.GameServer.Content
{
    public class Package : IPackage
    {
        public string PackagePath { get; private set; }
        public string PackageName { get; private set; }

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

        public Package(string packageName, string packagePath, Game game)
        {
            PackageName = packageName;
            PackagePath = packagePath;
            
            _game = game;
            _logger = LoggerProvider.GetLogger();

            InitializeContent();

            LoadPackage();
            LoadItems();
            LoadScripts();
        }

        public IContentFile GetContentFileFromJson(string contentType, string itemName)
        {
            if (!_content.ContainsKey(contentType) || !_content[contentType].ContainsKey(itemName))
            {
                _logger.Debug($"Package: {PackageName} does not containt file for {itemName}.");
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

        public INavGrid GetNavGrid(int mapId)
        {
            var navgridPath = $"{PackagePath}/AIMesh/Map{mapId}/AIPath.aimesh_ngrid";

            if (!File.Exists(navgridPath))
            {
                _logger.Debug($"{PackageName} does not contain a navgrid, skipping...");
                return null;
            }

            return NavGridReader.ReadBinary(navgridPath);
        }

        private void InitializeContent()
        {
            foreach (var contentType in ContentTypes)
            {
                _content[contentType] = new Dictionary<string, List<string>>();
            }
        }

        public bool LoadScripts()
        {
            var scriptLoadResult = _game.ScriptEngine.LoadSubdirectoryScripts(PackagePath);

            if (scriptLoadResult)
            {
                _logger.Debug($"Loaded C# scripts from package: {PackageName}");
                return true;
            }

            _logger.Debug($"{PackageName} does not contain C# scripts, skipping...");
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
