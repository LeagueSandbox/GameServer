
using InibinSharp;
using InibinSharp.RAF;
using LeagueSandbox.GameServer.Logic;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Core.Logic.RAF
{
    class RAFManager
    {
        private RAFMasterFileList _root;
        private Logger _logger;

        public RAFManager(Logger _logger)
        {
            /* if (!Directory.Exists(rootDirectory))
                 return false;

             var dirs = Directory.EnumerateDirectories(rootDirectory);

             foreach (var dir in dirs)
             {
                 var files = Directory.GetFiles(dir, "*.raf");

                 foreach (var file in files)
                 {
                     if (!File.Exists(file))
                         continue;

                     var raf = RiotArchive.FromFile(file);
                     Files.Add(raf);
                 }
             }*/

            _logger.LogCoreInfo("Loading RAF files in filearchives/.");

            var settings = Settings.Load("Settings/Settings.json");

            _root = new RAFMasterFileList(
                System.IO.Path.Combine(settings.RadsPath, "filearchives")
            );

            _logger.LogCoreInfo("Loaded RAF files");
        }

        public string findGameBasePath()
        {
            var possiblePaths = new List<Tuple<string, string>>
            {
                new Tuple<string, string>(@"HKEY_CURRENT_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\SightstoneLol",
                    "Path"),
                new Tuple<string, string>(
                    @"HKEY_CURRENT_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\RIOT GAMES", "Path"),
                new Tuple<string, string>(
                    @"HKEY_CURRENT_USER\Software\Classes\VirtualStore\MACHINE\SOFTWARE\Wow6432Node\RIOT GAMES",
                    "Path"),
                new Tuple<string, string>(@"HKEY_CURRENT_USER\Software\RIOT GAMES", "Path"),
                new Tuple<string, string>(@"HKEY_CURRENT_USER\Software\Wow6432Node\Riot Games", "Path"),
                new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\Software\Riot Games\League Of Legends", "Path"),
                new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Riot Games", "Path"),
                new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Riot Games\League Of Legends",
                    "Path"),
                // Yes, a f*ckin whitespace after "Riot Games"..
                new Tuple<string, string>(@"HKEY_LOCAL_MACHINE\Software\Wow6432Node\Riot Games \League Of Legends",
                    "Path"),
            };

            foreach (var tuple in possiblePaths)
            {
                var path = tuple.Item1;
                var valueName = tuple.Item2;
                try
                {
                    var value = Registry.GetValue(path, valueName, string.Empty);
                    if (value == null || value.ToString() == string.Empty)
                        continue;

                    return Path.Combine(value.ToString(), "RADS", "projects", "lol_game_client");
                }
                catch
                {

                }
            }

            var findLeagueDialog = new OpenFileDialog();

            if (!Directory.Exists(Path.Combine("C:\\", "Riot Games", "League of Legends")))
                findLeagueDialog.InitialDirectory = Path.Combine("C:\\", "Program Files (x86)", "GarenaLoL", "GameData",
                    "Apps", "LoL");
            else
                findLeagueDialog.InitialDirectory = Path.Combine("C:\\", "Riot Games", "League of Legends");

            findLeagueDialog.DefaultExt = ".exe";
            findLeagueDialog.Filter = "League of Legends Launcher|lol.launcher*.exe|Garena Launcher|lol.exe";

            var result = findLeagueDialog.ShowDialog();
            if (result != true)
                return string.Empty;

            var p = findLeagueDialog.FileName.Replace("lol.launcher.exe", string.Empty).Replace("lol.launcher.admin.exe", string.Empty);
            try
            {
                var key = Registry.CurrentUser.CreateSubKey("Software\\RIOT GAMES");
                key.SetValue("Path", p);
            }
            catch { }

            p = Path.Combine(p, "RADS", "projects", "lol_game_client");
            _logger.LogCoreInfo("Found base path in " + p);

            return p;
        }

        internal List<RAFFileListEntry> SearchFileEntries(string path)
        {
            return _root.SearchFileEntries(path);
        }

        internal bool readInibin(string path, out Inibin iniFile)
        {
            if (_root == null)
            {
                iniFile = null;
                return false;
            }
            var entries = _root.SearchFileEntries(path);

            if (entries.Count < 1)
            {
                iniFile = null;
                return false;
            }
            if (entries.Count > 1)
                _logger.LogCoreInfo("Found more than one inibin for query " + path);

            var entry = entries.First();
            iniFile = new Inibin(entry);

            return true;
        }

        public Inibin GetAutoAttackData(string model)
        {
            Inibin autoAttack = null;
            string[] autoAttackPaths = new string[]
            {
                string.Format("DATA/Characters/{0}/{0}BasicAttack.inibin", model),
                string.Format("DATA/Characters/{0}/Spells/{0}BasicAttack.inibin", model),
                string.Format("DATA/Spells/{0}BasicAttack.inibin", model)
            };
            foreach (var path in autoAttackPaths)
            {
                if (readInibin(path, out autoAttack))
                {
                    break;
                }
            }
            if (autoAttack == null)
            {
                _logger.LogCoreError(string.Format("Couldn't find auto-attack data for {0}", model));
            }
            return autoAttack;
        }

        internal bool readAIMesh(string path, out LeagueSandbox.GameServer.Logic.RAF.AIMesh aimesh)
        {
            if (_root == null)
            {
                aimesh = null;
                return false;
            }

            var entries = _root.SearchFileEntries(path, RAFSearchType.End);
            if (entries.Count < 1)
            {
                aimesh = null;
                return false;
            }
            if (entries.Count > 1)
            {
                _logger.LogCoreError("Found " + entries.Count + " AIMesh for query " + path);
                foreach (var e in entries)
                {
                    _logger.LogCoreInfo(e.FileName);
                }
            }

            var entry = entries.First();
            aimesh = new LeagueSandbox.GameServer.Logic.RAF.AIMesh(entry);

            return true;
        }

        public uint getHash(string path)
        {
            uint hash = 0;
            uint mask = 0xF0000000;
            for (var i = 0; i < path.Length; i++)
            {
                hash = Char.ToLower(path[i]) + (0x10 * hash);
                if ((hash & mask) > 0)
                    hash ^= hash & mask ^ ((hash & mask) >> 24);
            }
            return hash;
        }
    }
}
