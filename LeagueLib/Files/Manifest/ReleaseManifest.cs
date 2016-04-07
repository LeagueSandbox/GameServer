using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LeagueLib.Utils;

namespace LeagueLib.Files.Manifest
{
    public class ReleaseManifest
    {
        public string FileLocation { get; set; }
        public ReleaseManifestHeader Header { get; set; }
        public StringTable Strings { get; set; }
        public ReleaseManifestDirectoryEntry Root { get; set; }
        public ReleaseManifestFileEntry[] Files { get; set; }
        public ReleaseManifestDirectoryEntry[] Directories { get; set; }

        public ReleaseManifest() { }

        public List<string> GetFilePaths(string filename, string foldername)
        {
            List<string> result = new List<string>();
            for(int i = 0; i < Files.Length; i++)
            {
                if (Files[i].FullName.Contains(foldername) && Path.GetFileName(Files[i].FullName.ToLower()) == filename.ToLower())
                    result.Add(Files[i].FullName);
            }
            return result;
        }

        public void SaveChanges()
        {
            var writer = new ReleaseManifestWriter(this);
            writer.Save(FileLocation);
        }

        public static ReleaseManifest LoadFromFile(string path)
        {
            var reader = new ReleaseManifestReader(path);
            return reader.Read();
        }

        public static ReleaseManifest Generate(string path, Dictionary<string, Archive> files, uint version)
        {
            var manifest = new ReleaseManifest();
            manifest.FileLocation = path;
            manifest.Header = new ReleaseManifestHeader();
            manifest.Header.EntityVersion = version;
            manifest.Header.FormatVersion = 65537;
            manifest.Header.Magic = 1297304658;
            manifest.Header.UnknownCount = 77266;
            //manifest.Directories = new ReleaseManifestDirectoryEntry[0];
            //manifest.Files = new ReleaseManifestFileEntry[0];
            //manifest.Strings = new StringTable();
            manifest.SaveChanges();

            var dirs = new List<ReleaseManifestDirectoryEntry>();

            var descriptor = new ReleaseManifestDirectoryDescriptor();
            new ReleaseManifestDirectoryEntry(0, manifest, descriptor, null);

            return manifest;
        }
    }
}
