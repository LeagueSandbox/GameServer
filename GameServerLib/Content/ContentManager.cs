using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Logging;
using log4net;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.GameServer.Content
{
    public class ContentManager
    {
        private readonly ILog _logger;
        private Game _game;

        private readonly Dictionary<string, ISpellData> _spellData = new Dictionary<string, ISpellData>();
        private Dictionary<string, CharData> _charData = new Dictionary<string, CharData>();

        private string _contentPath;

        private static readonly string[] ContentTypes = {
            "Champions",
            "Items",
            "Buffs",
            "Maps",
            "Spells",
            "Stats"
        };

        private Dictionary<string, Dictionary<string, List<string>>> _content;

        public List<string> DataPackageNames { get; private set; }

        private ContentManager(Game game, string dataPackageName, string contentPath)
        {
            _game = game;

            _contentPath = contentPath;

            DataPackageNames = new List<string>{dataPackageName};

            _logger = LoggerProvider.GetLogger();

            _content = new Dictionary<string, Dictionary<string, List<string>>>();

            foreach (var contentType in ContentTypes)
            {
                _content[contentType] = new Dictionary<string, List<string>>();
            }
        }

        private void AddContent(string packageName, string contentType)
        {
            var contentPath = GetContentSetPath(packageName, contentType);
            var contents = GetFolderNamesFromPath(contentPath);

            foreach (var content in contents)
            {
                _logger.Debug($"Mapped Content [{packageName}][{contentType}][{content}]");
                if (!_content[contentType].ContainsKey(content))
                {
                    _content[contentType][content] = new List<string>();
                }

                _content[contentType][content].Add(packageName);
            }
        }

        private string[] GetFolderNamesFromPath(string folderPath)
        {
            var contents = new List<string>();
            if (Directory.Exists(folderPath))
            {
                var contentDirectories = Directory.GetDirectories(folderPath);
                foreach (var directory in contentDirectories)
                {
                    contents.Add(directory.Replace('\\', '/').Split('/').Last());
                }
            }
            return contents.ToArray();
        }

        private string GetPackagePath(string packageName)
        {
            return $"{_contentPath}/Data/{packageName}";
        }

        private string GetContentSetPath(string packageName, string contentType)
        {
            return $"{GetPackagePath(packageName)}/{contentType}";
        }

        private string GetContentPath(string packageName, string contentType, string fileName)
        {
            return $"{GetContentSetPath(packageName, contentType)}/{fileName}";
        }

        private string GetContentPath(List<string> contentPackages, string contentType, string fileName)
        {
            var path = "";
            var depth = contentPackages.Count;
            while (!File.Exists(path) && depth > 0)
            {
                depth--;
                path = GetContentPath(contentPackages[depth], contentType, fileName);
            }

            if (!File.Exists(path))
            {
                throw new ContentNotFoundException("Failed to load content [" + contentType + "][" + fileName + "]");
            }

            _logger.Debug($"Loaded content [{contentPackages[depth]}][{contentType}][{fileName}]");
            return path;
        }

        public string GetMapDataPath(int mapId)
        {
            var mapName = $"Map{mapId}";
            var contentType = "Maps";

            if (!_content.ContainsKey(contentType) || !_content[contentType].ContainsKey(mapName))
            {
                throw new ContentNotFoundException($"Map{mapId} was not found in the files.");
            }

            var contentPackages = _content[contentType][mapName];
            var fileName = $"{mapName}/{mapName}.json";
            return GetContentPath(contentPackages, contentType, fileName);
        }

        public string GetSpellScriptPath(string championName, string spellSlot)
        {
            var contentType = "Champions";

            if (!_content.ContainsKey(contentType) || !_content[contentType].ContainsKey(championName))
            {
                throw new ContentNotFoundException($"{championName}/{spellSlot}.lua was not found.");
            }

            var contentPackages = _content[contentType][championName];
            var fileName = $"{championName}/{spellSlot}.lua";
            return GetContentPath(contentPackages, contentType, fileName);
        }

        public string GetBuffScriptPath(string buffName)
        {
            var contentType = "Buffs";

            if (!_content.ContainsKey(contentType) || !_content[contentType].ContainsKey(buffName))
            {
                throw new ContentNotFoundException($"Buff {buffName} was not found.");
            }

            var contentPackages = _content[contentType][buffName];
            var fileName = $"{buffName}/{buffName}.lua";
            return GetContentPath(contentPackages, contentType, fileName);
        }

        public string GetUnitStatPath(string model)
        {
            var contentType = "Stats";

            if (!_content.ContainsKey(contentType) || !_content[contentType].ContainsKey(model))
            {
                throw new ContentNotFoundException($"Stat file for {model} was not found.");
            }

            var contentPackages = _content[contentType][model];
            var fileName = $"{model}/{model}.json";
            return GetContentPath(contentPackages, contentType, fileName);
        }

        public string GetSpellDataPath(string spellName)
        {
            var contentType = "Spells";

            if (!_content.ContainsKey(contentType) || !_content[contentType].ContainsKey(spellName))
            {
                throw new ContentNotFoundException($"Spell data for {spellName} was not found.");
            }

            var contentPackages = _content[contentType][spellName];
            var fileName = $"{spellName}/{spellName}.json";

            return GetContentPath(contentPackages, contentType, fileName);
        }

        public ISpellData GetSpellData(string spellName)
        {
            if (_spellData.ContainsKey(spellName))
            {
                return _spellData[spellName];
            }

            _spellData[spellName] = new SpellData(_game);
            _spellData[spellName].Load(spellName);
            return _spellData[spellName];
        }

        public CharData GetCharData(string charName)
        {
            if (_charData.ContainsKey(charName))
            {
                return _charData[charName];
            }
            _charData[charName] = new CharData(_game);
            _charData[charName].Load(charName);
            return _charData[charName];
        }

        public static ContentManager LoadDataPackage(Game game, string dataPackageName, string contentPath)
        {
            var contentManager = new ContentManager(game, dataPackageName, contentPath);

            List<string> extraPackageList = new List<string>();
            contentManager.GetDependenciesRecursively(extraPackageList, dataPackageName, contentPath);
            contentManager.DataPackageNames.AddRange(extraPackageList);

            foreach (var dataPackage in contentManager.DataPackageNames)
            {
                foreach (var contentType in ContentTypes)
                {
                    contentManager.AddContent(dataPackage, contentType);
                }
            }

            return contentManager;
        }

        private static bool ValidatePackageName(string packageName)
        {
            if (packageName.Equals("Self"))
            {
                return true;
            }

            if (packageName.All(c => c != '-'))
            {
                return false;
            }

            var parts = packageName.Split('-');
            foreach (var part in parts)
            {
                if (part.Length < 2)
                {
                    return false;
                }
            }

            return true;
        }

        private void GetDependenciesRecursively(List<string> resultList, string packageName, string contentPath)
        {
            foreach(var dependency in GetDependenciesFromPackage(packageName, contentPath))
            {
                if (!resultList.Contains(dependency))
                {
                    resultList.Add(dependency);

                    GetDependenciesRecursively(resultList, dependency, contentPath);
                }
            }
        }

        private List<string> GetDependenciesFromPackage(string packageName, string contentPath)
        {
            List<string> dependencyList = new List<string>();

            var dataPackageConfigurationPath = $"{contentPath}/Data/{packageName}/packageInfo.json";
            var dataPackageConfiguration = JToken.Parse(File.ReadAllText(dataPackageConfigurationPath));
            var dataPackageDependencies = dataPackageConfiguration.SelectToken("dependencies");

            foreach (var dependencyToken in dataPackageDependencies)
            {
                var dependencyName = dependencyToken.Value<string>();

                if (ValidatePackageName(dependencyName))
                {
                    dependencyList.Add(dependencyName);
                }
            }

            return dependencyList;
        }
    }
}
