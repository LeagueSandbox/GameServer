using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GameServerCore.Content;
using GameServerCore.Domain;
using LeagueSandbox.GameServer.Logging;
using log4net;
using Newtonsoft.Json.Linq;

namespace LeagueSandbox.GameServer.Content
{
    public class ContentManager : IPackage
    {
        private readonly ILog _logger;
        private readonly Game _game;
        private readonly string _contentPath;

        private readonly Dictionary<string, ISpellData> _spellData = new Dictionary<string, ISpellData>();
        private readonly Dictionary<string, CharData> _charData = new Dictionary<string, CharData>();

        private readonly List<Package> _loadedPackages;
        private readonly List<string> _dataPackageNames;

        public string PackageName { get; }
        public string PackagePath { get; }

        private ContentManager(Game game, string dataPackageName, string contentPath)
        {
            _game = game;

            _contentPath = contentPath;

            _loadedPackages = new List<Package>();
            _dataPackageNames = new List<string>{dataPackageName};

            _logger = LoggerProvider.GetLogger();
        }

        public Package GetLoadedPackage(string packageName)
        {
            foreach (var dataPackage in _loadedPackages)
            {
                if (dataPackage.PackageName.Equals(packageName))
                {
                    return dataPackage;
                }
            }

            return null;
        }

        public List<Package> GetAllLoadedPackages()
        {
            return _loadedPackages;
        }

        private void LoadPackages(string packageName)
        {
            string packagePath = GetPackagePath(packageName);

            Package dataPackage = new Package(packageName, packagePath, _game);

            if (_loadedPackages.Contains(dataPackage))
            {
                return;
            }

            _loadedPackages.Add(dataPackage);
        }

        private string GetPackagePath(string packageName)
        {
            return $"{_contentPath}/{packageName}";
        }

        public bool LoadScripts()
        {
            List<bool> packageLoadingResults = new List<bool>();

            foreach (var dataPackage in _loadedPackages)
            {
                packageLoadingResults.Add(dataPackage.LoadScripts());
            }

            return packageLoadingResults.Contains(false);
        }

        public MapSpawns GetMapSpawns(int mapId)
        {
            foreach (var dataPackage in _loadedPackages)
            {
                var toReturnMapSpawns = dataPackage.GetMapSpawns(mapId);

                if (toReturnMapSpawns == null)
                {
                    continue;
                }

                return toReturnMapSpawns;
            }

            throw new ContentNotFoundException($"No map spawns found for map with id: {mapId}");
        }

        public IContentFile GetContentFileFromJson(string contentType, string itemName)
        {
            foreach (var dataPackage in _loadedPackages)
            {
                var toReturnContentFile = dataPackage.GetContentFileFromJson(contentType, itemName);

                if (toReturnContentFile == null)
                {
                    continue;
                }

                return (ContentFile)toReturnContentFile;
            }

            throw new ContentNotFoundException($"No {contentType} found with name: {itemName} in any package.");
        }

        public INavGrid GetNavGrid(int mapId)
        {
            foreach (var dataPackage in _loadedPackages)
            {
                INavGrid toReturnNavgrid = dataPackage.GetNavGrid(mapId);

                if (toReturnNavgrid != null)
                {
                    return toReturnNavgrid;
                }
            }

            throw new ContentNotFoundException($"No NavGrid for map with id {mapId} found in packages, skipping map load...");
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

            foreach (var packageName in extraPackageList)
            {
                if (!contentManager._dataPackageNames.Contains(packageName))
                {
                    contentManager._dataPackageNames.Add(packageName);
                }
            }

            foreach (var dataPackage in contentManager._dataPackageNames)
            {
                contentManager.LoadPackages(dataPackage);

                contentManager._logger.Debug($"Loaded package with name: {dataPackage}");
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

            var dataPackageConfigurationPath = $"{contentPath}/{packageName}/packageInfo.json";
            JToken dataPackageConfiguration = null;

            try
            {
                dataPackageConfiguration = JToken.Parse(File.ReadAllText(dataPackageConfigurationPath));
            }
            catch (FileNotFoundException)
            {
                _logger.Debug($"{dataPackageConfigurationPath} not found, skipping...");
                return new List<string>();
            }

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
