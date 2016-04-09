using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic;
using LeagueLib.Tools;

namespace ContentExtractor
{
    class Program
    {
        static void Main(string[] args)
        {
            var settings = Settings.Load("Settings/Settings.json");
            var manager = new ArchiveFileManager(settings.RadsPath);
            var fileEntries = manager.GetAllFileEntries().OrderBy(x => x.FullName);

            foreach (var entry in fileEntries)
            {
                Console.WriteLine(entry.FullName);
            }
        }
    }
}
