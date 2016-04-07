using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueLib.Files
{
    public class ArchiveState
    {
        public string ArchivePath { get; set; }
        public long OriginalLength { get; set; }
        public Dictionary<string, ArchiveFileInfo> OriginalValues { get; set; }
    }
}
