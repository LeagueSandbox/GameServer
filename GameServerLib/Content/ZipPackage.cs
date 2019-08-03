using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServerCore.Content;
using GameServerCore.Domain;
using log4net;
using LeagueSandbox.GameServer.Logging;

namespace LeagueSandbox.GameServer.Content
{
    public class ZipPackage : IPackage
    {
        public string PackageName { get; private set; }
        public string PackagePath { get; private set; }

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

        public ZipPackage(string zipPackagePath, Game game)
        {
            PackagePath = zipPackagePath;

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

        private void LoadPackage()
        {
            foreach (var contentType in ContentTypes)
            {
                LoadData(contentType);
            }
        }

        private void LoadData(string contentType)
        {
            if (!File.Exists(PackagePath))
            {
                return;
            }

            var contentTypeFolder = $"{PackageName}/{contentType}/";

            using (var archive = ZipFile.OpenRead(PackagePath))
            {
                foreach (var entry in archive.Entries)
                {
                    if (!entry.FullName.StartsWith(contentTypeFolder) || entry.FullName.Equals(contentTypeFolder))
                    {
                        continue;
                    }

                    if (entry.FullName.EndsWith(".json"))
                    {
                        var fileName = Path.GetFileNameWithoutExtension(entry.FullName);

                        if (fileName == null)
                        {
                            continue;
                        }

                        _content[contentType][fileName] = new List<string> { PackageName };
                    }
                }
            }
        }

        private void LoadItems()
        {

        }

        public bool LoadScripts()
        {
            return false;
        }

        public IContentFile GetContentFileFromJson(string contentType, string itemName)
        {
            return null;
        }

        public INavGrid GetNavGrid(int mapId)
        {
            return null;
        }

        public ISpellData GetSpellData(string spellName)
        {
            return null;
        }

        public ICharData GetCharData(string characterName)
        {
            return null;
        }

        private string GetContentTypePath(string contentType)
        {
            return $"{PackagePath}/{contentType}";
        }
    }
}
