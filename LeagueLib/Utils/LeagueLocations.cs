using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Win32;

namespace LeagueLib.Utils
{
    public static class LeagueLocations
    {
        public static string GetLeaguePath()
        {
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall");
            return FindInstallataionLocation(regKey, "League of Legends");
        }

        public static string GetArchivePath(string leaguePath)
        {
            return string.Format(leaguePath + @"\filearchives\");
        }

        public static string GetManifestPath(string leaguePath)
        {
            string manifestDir = string.Format(leaguePath + @"\releases\");
            string[] directories = Directory.GetDirectories(manifestDir);
            int currentResult = 0;
            int[] currentVersion = new int[4];

            for(int i = 0; i < directories.Length; i++)
            {
                string folder = directories[i].Substring(directories[i].LastIndexOf('\\') + 1);
                string[] version = folder.Split('.');
                for(int j = 0; j < version.Length; j++)
                {
                    if (Convert.ToInt32(version[j]) < currentVersion[j])
                        break;

                    if(Convert.ToInt32(version[j]) > currentVersion[j])
                    {
                        currentResult = i;
                        for (int k = 0; k < version.Length; k++)
                        {
                            currentVersion[k] = Convert.ToInt32(version[k]);
                        }
                        break;
                    }
                }
            }
            return directories[currentResult] + @"\releasemanifest";
        }

        public static string GetBackupPath(string leaguePath)
        {
            return GetModPath(leaguePath) + @"Backup\";
        }

        public static string GetModPath(string leaguePath)
        {
            return leaguePath + @"Mythic\";
        }

        public static string GetManifestStatePath(string leaguePath)
        {
            return GetModPath(leaguePath) + @"ManifestState.sif";
        }

        public static string GetArchiveStatePath(string leaguePath)
        {
            return GetModPath(leaguePath) + @"ArchiveStates.sif";
        }

        public static string GetCorruptFlagPath(string leaguePath)
        {
            return leaguePath + @"\releases\0.0.1.11\SOFT_REPAIR";
        }

        private static string FindInstallataionLocation(RegistryKey parentKey, string name)
        {
            string[] nameList = parentKey.GetSubKeyNames();
            for (int i = 0; i < nameList.Length; i++)
            {
                RegistryKey key = parentKey.OpenSubKey(nameList[i]);
                try
                {
                    if (key.GetValue("DisplayName").ToString() == name)
                    {
                        return key.GetValue("InstallLocation").ToString();
                    }
                }
                catch { }
            }
            return "";
        }
    }
}
