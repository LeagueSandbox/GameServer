using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeagueSandbox.GameServer.Content.Navigation;
using LeagueSandbox.GameServer.Handlers;
using LeagueSandbox.GameServer.Logging;
using log4net;
using Newtonsoft.Json.Linq;
using static LeagueSandbox.GameServer.Content.TalentContentCollection;

namespace LeagueSandbox.GameServer.Content
{
    public class ContentManager
    {
        private static ILog _logger = LoggerProvider.GetLogger();
        private readonly Game _game;

        private readonly List<Package> _loadedPackages;
        private readonly List<string> _dataPackageNames;

        public string ContentPath { get; }
        public string PackageName { get; }
        public string PackagePath { get; }

        private ContentManager(Game game, string dataPackageName, string contentPath)
        {
            _game = game;

            ContentPath = contentPath;

            _loadedPackages = new List<Package>();
            _dataPackageNames = new List<string> { dataPackageName };
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
                contentManager.LoadPackage(dataPackage);

                _logger.Debug($"Loaded package with name: {dataPackage}");
            }

            return contentManager;
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

        public void LoadPackage(string packageName)
        {
            string packagePath = GetPackagePath(packageName);

            Package dataPackage = new Package(packagePath, _game);

            dataPackage.LoadPackage(packageName);

            if (_loadedPackages.Contains(dataPackage))
            {
                return;
            }

            _loadedPackages.Add(dataPackage);
        }

        private string GetPackagePath(string packageName)
        {
            return $"{ContentPath}/{packageName}";
        }

        public bool HasScripts()
        {
            foreach (var dataPackage in _loadedPackages)
            {
                if (dataPackage.HasScripts())
                {
                    return true;
                }
            }

            return false;
        }

        public bool LoadScripts()
        {
            bool packageLoadingResults = true;

            foreach (var dataPackage in _loadedPackages)
            {
                packageLoadingResults = packageLoadingResults && dataPackage.LoadScripts();
            }

            return packageLoadingResults;
        }

        public MapData GetMapData(int mapId)
        {
            foreach (var dataPackage in _loadedPackages)
            {
                var toReturnMapData = dataPackage.GetMapData(mapId);

                if (toReturnMapData == null)
                {
                    continue;
                }

                return toReturnMapData;
            }

            throw new ContentNotFoundException($"No map data found for map with id: {mapId}");
        }

        public Dictionary<string, JArray> GetMapSpawns(int mapId)
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

        public NavigationGrid GetNavigationGrid(MapScriptHandler map)
        {
            foreach (var dataPackage in _loadedPackages)
            {
                NavigationGrid toReturnNavgrid = dataPackage.GetNavigationGrid(map);

                if (toReturnNavgrid != null)
                {
                    return toReturnNavgrid;
                }
            }

            throw new ContentNotFoundException($"No NavGrid for map with id {map.Id} found in packages, skipping map load...");
        }

        public SpellData GetSpellData(string spellName)
        {
            foreach (var dataPackage in _loadedPackages)
            {
                SpellData toReturnSpellData = dataPackage.GetSpellData(spellName);

                if (toReturnSpellData != null)
                {
                    return toReturnSpellData;
                }
            }

            throw new ContentNotFoundException($"No Spell Data found with name: {spellName}");
        }

        public CharData GetCharData(string characterName)
        {
            foreach (var dataPackage in _loadedPackages)
            {
                CharData toReturnCharData = dataPackage.GetCharData(characterName);

                if (toReturnCharData != null)
                {
                    return toReturnCharData;
                }
            }

            throw new ContentNotFoundException($"No Character found with name: {characterName}");
        }

        public TalentCollectionEntry GetTalentEntry(string talentName)
        {
            foreach (var dataPackage in _loadedPackages)
            {
                TalentCollectionEntry toReturn = dataPackage.GetTalentEntry(talentName);
                if (toReturn != null)
                {
                    return toReturn;
                }
            }

            return null;
        }

        private void GetDependenciesRecursively(List<string> resultList, string packageName, string contentPath)
        {
            foreach (var dependency in GetDependenciesFromPackage(packageName, contentPath))
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

    }
}
