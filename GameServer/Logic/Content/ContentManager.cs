using LeagueSandbox.GameServer.Core.Logic;
using Newtonsoft.Json.Linq;
using Ninject;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Content
{
    public class ContentManager
    {
        private Logger _logger = Program.ResolveDependency<Logger>();

        private static readonly string[] CONTENT_TYPES = new string[]
        {
            "Champions",
            "Items",
            "Buffs",
            "Maps",
            "Spells",
            "Stats"
        };

        private Dictionary<string, Dictionary<string, List<string>>> _content;
        public string GameModeName { get; }

        private ContentManager(string gameModeName)
        {
            GameModeName = gameModeName;

            _content = new Dictionary<string, Dictionary<string, List<string>>>();
            foreach(var contentType in CONTENT_TYPES)
            {
                _content[contentType] = new Dictionary<string, List<string>>();
            }
        }

        private void AddContent(string packageName, string contentType, JToken contentSet)
        {
            var contents = new string[0];
            if (contentSet is JArray)
            {
                contents = contentSet.ToObject<string[]>();
            }
            else if(contentSet.Value<string>() == "*")
            {
                var contentPath = GetContentSetPath(packageName, contentType);
                contents = GetFolderNamesFromPath(contentPath);
            }
            else
            {
                throw new Exception("Invalid content configuration");
            }

            foreach(var content in contents)
            {
                _logger.LogCoreInfo("Mapped Content [{0}][{1}][{2}]", packageName, contentType, content);
                if(!_content[contentType].ContainsKey(content))
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
                    contents.Add(directory.Split('\\').Last());
                }
            }
            return contents.ToArray();
        }

        private string GetContentRootPath()
        {
            return "Content";
        }

        private string GetPackagePath(string packageName)
        {
            return string.Format("{0}/Data/{1}", GetContentRootPath(), packageName);
        }

        private string GetContentSetPath(string packageName, string contentType)
        {
            if(packageName == "Self")
            {
                return string.Format("{0}/GameMode/{1}/Data/{2}", GetContentRootPath(), GameModeName, contentType);
            }
            return string.Format("{0}/{1}", GetPackagePath(packageName), contentType);
        }

        private string GetContentPath(string packageName, string contentType, string fileName)
        {
            return string.Format("{0}/{1}", GetContentSetPath(packageName, contentType), fileName);
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
            _logger.LogCoreInfo("Loaded content [{0}][{1}][{2}]", contentPackages[depth], contentType, fileName);
            return path;
        }

        public string GetMapDataPath(int mapId)
        {
            var mapName = string.Format("Map{0}", mapId);
            var contentType = "Maps";

            if (!_content.ContainsKey(contentType) || !_content[contentType].ContainsKey(mapName))
            {
                throw new ContentNotFoundException();
            }

            var contentPackages = _content[contentType][mapName];
            var fileName = string.Format("{0}/{0}.json", mapName);
            return GetContentPath(contentPackages, contentType, fileName);
        }

        public string GetSpellScriptPath(string championName, string spellSlot)
        {
            var contentType = "Champions";

            if (!_content.ContainsKey(contentType) || !_content[contentType].ContainsKey(championName))
            {
                throw new ContentNotFoundException();
            }

            var contentPackages = _content[contentType][championName];
            var fileName = string.Format("{0}/{1}.lua", championName, spellSlot);
            return GetContentPath(contentPackages, contentType, fileName);
        }

        public string GetBuffScriptPath(string buffName)
        {
            var contentType = "Buffs";

            if (!_content.ContainsKey(contentType) || !_content[contentType].ContainsKey(buffName))
            {
                throw new ContentNotFoundException();
            }

            var contentPackages = _content[contentType][buffName];
            var fileName = string.Format("{0}/{1}.lua", buffName, buffName);
            return GetContentPath(contentPackages, contentType, fileName);
        }

        public string GetUnitStatPath(string model)
        {
            var contentType = "Stats";

            if (!_content.ContainsKey(contentType) || !_content[contentType].ContainsKey(model))
            {
                throw new ContentNotFoundException();
            }

            var contentPackages = _content[contentType][model];
            var fileName = string.Format("{0}/{1}.json", model, model);
            return GetContentPath(contentPackages, contentType, fileName);
        }

        public string GetSpellDataPath(string spellName)
        {
            var contentType = "Spells";

            if (!_content.ContainsKey(contentType) || !_content[contentType].ContainsKey(spellName))
            {
                throw new ContentNotFoundException();
            }

            var contentPackages = _content[contentType][spellName];
            var fileName = string.Format("{0}/{1}.json", spellName, spellName);

            return GetContentPath(contentPackages, contentType, fileName);
        }

        public static ContentManager LoadGameMode(string gameModeName)
        {
            var contentManager = new ContentManager(gameModeName);

            var gameModeConfigurationPath = string.Format("Content/GameMode/{0}/GameMode.json", gameModeName);
            var gameModeConfiguration = JToken.Parse(File.ReadAllText(gameModeConfigurationPath));
            var dataConfiguration = gameModeConfiguration.SelectToken("data");

            foreach(JProperty dataPackage in dataConfiguration)
            {
                if (!ValidatePackageName(dataPackage.Name)) throw new Exception("Data packages must be namespaced!");

                foreach(var contentType in CONTENT_TYPES)
                {
                    var contentSet = dataPackage.Value.SelectToken(contentType);

                    if (contentSet == null) continue;

                    contentManager.AddContent(dataPackage.Name, contentType, contentSet);
                }
            }

            return contentManager;
        }

        private static bool ValidatePackageName(string packageName)
        {
            if (packageName == "Self") return true;

            if (packageName.Count(c => c == '-') < 1) return false;
            string[] parts = packageName.Split('-');
            foreach(var part in parts)
            {
                if (part.Length < 2) return false;
            }
            return true;
        }
    }
}
