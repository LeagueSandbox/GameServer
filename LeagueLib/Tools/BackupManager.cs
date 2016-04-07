using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LeagueLib.Utils;

namespace LeagueLib.Tools
{
    public class BackupManager
    {
        private string _leaguePath;
        private string _archivePath;
        private string _backupPath;
        private string _manifestPath;

        public BackupManager(string leaguePath)
        {
            _leaguePath = leaguePath;
            _archivePath = LeagueLocations.GetArchivePath(leaguePath);
            _backupPath = LeagueLocations.GetBackupPath(leaguePath);
            _manifestPath = LeagueLocations.GetManifestPath(leaguePath);
        }

        public bool GetBackupState()
        {
            return Directory.Exists(_backupPath) && Directory.EnumerateFiles(_backupPath, "*.raf", SearchOption.AllDirectories).ToArray().Length > 0;
        }

        public void Backup(bool force)
        {
            var title = Console.Title;
            var archives = Directory.EnumerateFiles(_archivePath, "*.raf", SearchOption.AllDirectories).ToArray();

            for (int i = 0; i < archives.Length; i++)
            {
                Console.Title = string.Format("Backing up archives... {0} / {1} done", i + 1, archives.Length);

                var target = _backupPath + archives[i].Remove(0, _archivePath.Length);

                if (!Directory.Exists(Path.GetDirectoryName(target)))
                    Directory.CreateDirectory(Path.GetDirectoryName(target));

                CopyFile(archives[i], target, force, false);
                CopyFile(archives[i] + ".dat", target + ".dat", force, false);
            }

            CopyFile(_manifestPath, _backupPath + "releasemanifest", force, false);

            Console.Title = title;
        }

        public void Restore(bool removeBackup)
        {
            var title = Console.Title;
            var archives = Directory.EnumerateFiles(_backupPath, "*.raf", SearchOption.AllDirectories).ToArray();

            for (int i = 0; i < archives.Length; i++)
            {
                Console.Title = string.Format("Restoring archives... {0} / {1} done", i + 1, archives.Length);

                var target = _archivePath + archives[i].Remove(0, _backupPath.Length);

                if (!Directory.Exists(Path.GetDirectoryName(target)))
                    Directory.CreateDirectory(Path.GetDirectoryName(target));

                CopyFile(archives[i], target, true, removeBackup);
                CopyFile(archives[i] + ".dat", target + ".dat", true, removeBackup);
            }

            CopyFile(_backupPath + "releasemanifest", _manifestPath, true, removeBackup);

            if (File.Exists(LeagueLocations.GetCorruptFlagPath(_leaguePath)))
                File.Delete(LeagueLocations.GetCorruptFlagPath(_leaguePath));

            Console.Title = title;
        }

        private void CopyFile(string source, string target, bool force, bool deleteSource)
        {
            if (File.Exists(source))
            {
                if (File.Exists(target) && force)
                    File.Delete(target);

                if (!File.Exists(target))
                    File.Copy(source, target);

                if (deleteSource)
                    File.Delete(source);
            }
            else
            {
                Console.WriteLine("Copy source file was not found: {0}", source);
            }
        }
    }
}
