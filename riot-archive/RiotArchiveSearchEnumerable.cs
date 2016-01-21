using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace freedompeace.RiotArchive
{
    internal class RiotArchiveSearchEnumerable : IEnumerable<RiotArchiveFile>
    {
        readonly IEnumerable<RiotArchiveFile> files;
        readonly Regex searchPattern;

        public RiotArchiveSearchEnumerable(IEnumerable<RiotArchiveFile> files, string searchPattern)
        {
            this.files = files;
            this.searchPattern = new Regex(searchPattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Singleline);
        }

        public IEnumerator<RiotArchiveFile> GetEnumerator()
        {
            return new RiotArchiveSearchEnumerator(files.GetEnumerator(), searchPattern.IsMatch);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
