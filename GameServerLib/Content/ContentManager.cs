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
        public List<string> DataPackageNames { get; private set; }

        private readonly ILog _logger;
        private readonly Game _game;
        private readonly string _contentPath;

        private readonly Dictionary<string, ISpellData> _spellData = new Dictionary<string, ISpellData>();
        private Dictionary<string, CharData> _charData = new Dictionary<string, CharData>();
        private readonly List<IPackage> loadedPackages = new List<IPackage>();

        private ContentManager(Game game, string dataPackageName, string contentPath)
        {
            _game = game;

            _contentPath = contentPath;

            DataPackageNames = new List<string>{dataPackageName};

            _logger = LoggerProvider.GetLogger();
        }

        private void LoadPackages(string packageName)
        {
            string packagePath = GetPackagePath(packageName);

            Package dataPackage = new Package(packageName, packagePath, _game);

            loadedPackages.Add(dataPackage);
        }

        private string GetPackagePath(string packageName)
        {
            return $"{_contentPath}/{packageName}";
        }

        public List<bool> ReloadScripts()
        {
            List<bool> packageLoadingResults = new List<bool>();

            foreach (var dataPackage in loadedPackages)
            {
                packageLoadingResults.Add(dataPackage.LoadScripts());
            }

            return packageLoadingResults;
        }

        public JToken GetMapSpawnData(int mapId)
        {
            foreach (var dataPackage in loadedPackages)
            {
                var toReturnContentFile = dataPackage.GetMapSpawnData(mapId);

                if (toReturnContentFile == null)
                {
                    continue;
                }

                return toReturnContentFile;
            }

            throw new ContentNotFoundException($"No map found with id: {mapId}");
        }

        public ContentFile GetUnitStatFile(string unitName)
        {
            foreach (var dataPackage in loadedPackages)
            {
                var toReturnContentFile = dataPackage.GetContentFileFromJson("Stats", unitName);

                if (toReturnContentFile == null)
                {
                    continue;
                }

                return (ContentFile) toReturnContentFile;
            }

            throw new ContentNotFoundException($"No unit found with name: {unitName}");
        }

        public ContentFile GetSpellDataFile(string spellName)
        {
            foreach (var dataPackage in loadedPackages)
            {
                var toReturnContentFile = dataPackage.GetContentFileFromJson("Spells", spellName);

                if (toReturnContentFile == null)
                {
                    continue;
                }

                return (ContentFile) toReturnContentFile;
            }

            throw new ContentNotFoundException($"No spell found with name: {spellName}");
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
                if (!contentManager.DataPackageNames.Contains(packageName))
                {
                    contentManager.DataPackageNames.Add(packageName);
                }
            }

            foreach (var dataPackage in contentManager.DataPackageNames)
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
                Console.WriteLine($"{dataPackageConfigurationPath} not found, skipping...");
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
