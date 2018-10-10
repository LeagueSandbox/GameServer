using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using IniParser.Model;
using LeagueSandbox.GameServer.Logging;
using log4net;
using LeagueSandbox.GameServer.Exceptions;

namespace LeagueSandbox.GameServer.Content
{
    public class ContentManager
    {
        private readonly ILog _logger;
        private Game _game;

        private Dictionary<string, SpellData> _spellData = new Dictionary<string, SpellData>();
        private Dictionary<string, CharData> _charData = new Dictionary<string, CharData>();
        private Dictionary<string, NavGrid> _navGrids = new Dictionary<string, NavGrid>();

        public Dictionary<string, FileEntry> Content { get; set; }
            = new Dictionary<string, FileEntry>(StringComparer.InvariantCultureIgnoreCase);

        public string GameModeName { get; }

        private ContentManager(Game game, string gameModeName)
        {
            _game = game;
            _logger = LoggerProvider.GetLogger();

            GameModeName = gameModeName;
        }

        public string GetMapConfigPath(int mapId)
        {
            var path = $"LEVELS/Map{mapId}/Map{mapId}.json";
            if (!Content.ContainsKey(path))
            {
                throw new ContentNotFoundException($"Map configuration for Map {mapId} was not found in the content.");
            }

            return path;
        }

        public string GetUnitStatPath(string model)
        {
            var path = $"DATA/Characters/{model}/{model}.ini";
            if (!Content.ContainsKey(path))
            {
                throw new ContentNotFoundException($"Stat file for {model} was not found.");
            }

            return path;
        }

        public string GetSpellDataPath(string model, string spellName)
        {
            var possibilities = new[]
            {
                $"DATA/Characters/{model}/Spells/{spellName}.ini",
                $"DATA/Shared/Spells/{spellName}.ini",
                $"DATA/Spells/{spellName}.ini"
            };

            return possibilities.FirstOrDefault(path => Content.ContainsKey(path))
                ?? throw new ContentNotFoundException($"Spell data for {spellName} was not found.");
        }

        public SpellData GetSpellData(string champ, string spellName)
        {
            if (_spellData.ContainsKey(spellName))
            {
                return _spellData[spellName];
            }

            _spellData[spellName] = new SpellData(_game);
            _spellData[spellName].Load(champ, spellName);
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

        public NavGrid GetNavGrid(int mapId)
        {
            var path = $"LEVELS/Map{mapId}/AIPath.aimesh_ngrid";

            if (!_navGrids.ContainsKey(path))
            {
                throw new ContentNotFoundException($"NavGrid for map {mapId} was not loaded.");
            }

            return _navGrids[path];
        }

        public static ContentManager LoadGameMode(Game game, string gameModeName, string contentPath)
        {
            var contentManager = new ContentManager(game, gameModeName);

            var zipPath = Path.Combine(contentPath, gameModeName, gameModeName + ".zip");

            // If zip exists
            if (File.Exists(zipPath))
            {
                // Read zip file data
                using (var file = File.OpenRead(zipPath))
                {
                    // Read archive data
                    using (var zip = new ZipArchive(file, ZipArchiveMode.Read))
                    {
                        // For every entry in the zip
                        foreach (var entry in zip.Entries)
                        {
                            // If file is aimesh_ngrid, load it up
                            if (entry.FullName.EndsWith(".aimesh_ngrid"))
                            {
                                using (var reader = new BinaryReader(entry.Open()))
                                {
                                    contentManager._navGrids[entry.FullName] =
                                        NavGridReader.ReadBinary(reader.ReadBytes((int)entry.Length));
                                }
                            }
                            // if file is cs and is not in either "bin" or "obj" folder, load it
                            else if (entry.FullName.EndsWith(".cs"))
                            {
                                if (entry.FullName.StartsWith("bin") || entry.FullName.StartsWith("obj"))
                                {
                                    continue;
                                }
                            }
                            // if file is ini or json, load them too, otherwise skip it
                            else if (!entry.FullName.EndsWith(".ini") && !entry.FullName.EndsWith(".json"))
                            {
                                continue;
                            }

                            // loading is put here to have less duplicate code
                            contentManager.Content[entry.FullName] =
                                new FileEntry(entry.Name, entry.FullName, entry.Open());

                            contentManager._logger.Debug($"Mapped content from zip [{entry.FullName}]");
                        }
                    }
                }
            }

            // Read non-zipped data
            foreach (var file in Directory.GetFiles(contentPath, "*.*", SearchOption.AllDirectories))
            {
                var relativePath = file.Replace(contentPath, "").Replace(gameModeName, "").Substring(2)
                    .Replace('\\', '/');

                if (file.EndsWith(".cs"))
                {
                    if (relativePath.StartsWith("bin") || relativePath.StartsWith("obj"))
                    {
                        continue;
                    }
                }
                else if (file.EndsWith(".aimesh_ngrid"))
                {
                    contentManager._navGrids[relativePath] = NavGridReader.ReadBinary(file);
                }
                else if (!file.EndsWith(".ini") && !file.EndsWith(".json"))
                {
                    continue;
                }

                var fileName = relativePath.Split('/').Last();
                contentManager.Content[relativePath] =
                    new FileEntry(fileName, file, File.Open(file, FileMode.Open));

                contentManager._logger.Debug($"Mapped Content [{relativePath}]");
            }

            return contentManager;
        }

        public static Dictionary<string, Dictionary<string, string>> ParseIniFile(IniData data)
        {
            var ret = new Dictionary<string, Dictionary<string, string>>();
            foreach (var section in data.Sections)
            {
                if (!ret.ContainsKey(section.SectionName))
                {
                    ret[section.SectionName] = new Dictionary<string, string>();
                }

                foreach (var field in section.Keys)
                {
                    ret[section.SectionName][field.KeyName] = field.Value;
                }
            }

            return ret;
        }

        private static bool ValidatePackageName(string packageName)
        {
            if (packageName.Equals("Self"))
            {
                return true;
            }

            if (!packageName.Contains('-'))
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
