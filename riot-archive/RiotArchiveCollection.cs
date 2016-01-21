using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freedompeace.RiotArchive
{
    /// <summary>
    /// Provides read access to a collection of Riot Archive Files, used in the League of Legends
    /// game. Thead-safe.
    /// </summary>
    public class RiotArchiveCollection
    {
        public RiotArchive[] Archives { get; private set; }

        bool initialised;
        
        /// <summary>
        /// Initialises a new instance of the RiotArchiveCollection class with the specified
        /// Riot Archive Files.
        /// </summary>
        /// <param name="archivePaths">A list of path names to open as Riot Archive Files.</param>
        public RiotArchiveCollection(IEnumerable<string> archivePaths)
        {
            this.Archives = archivePaths.Select(path => new RiotArchive(path)).ToArray();
        }

        /// <summary>
        /// Opens and loads the Riot Archive Files from the disk.
        /// </summary>
        public void Open()
        {
            if (initialised)
                return;
            initialised = true;

            foreach (var archive in Archives)
                archive.Open();
        }

        /// <summary>
        /// Determines whether the given path refers to an existing file in the archive.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns><value>true</value> if path refers to an existing directory; otherwise, <value>false</value></returns>
        public bool Exists(string path)
        {
            return Archives.Any(a => a.Exists(path));
        }

        public IEnumerable<RiotArchiveFile> Files
        {
            get { return Archives.SelectMany(x => x.Files); } }

        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path.
        /// </summary>
        /// <param name="searchPattern">The regular expression to match against the names of directories in path.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by <paramref name="path"/> and that match the specified search pattern.</returns>
        /// <remarks>The order in which files are returned is unspecified (unordered).</remarks>
        public IEnumerable<RiotArchiveFile> GetFiles(string searchPattern)
        {
            return Archives.SelectMany(a => a.GetFiles(searchPattern));
        }

        /// <summary>
        /// Gets the RiotArhiveFile associated with the specified path.
        /// </summary>
        /// <param name="path">The path of the RiotArchiveFile to get.</param>
        /// <returns></returns>
        public RiotArchiveFile this[string path]
        {
            get
            {
                return Archives.First(a => a.Exists(path))[path];
            }
        }

        /// <summary>
        /// Gets the RiotArhiveFile associated with the specified path.
        /// </summary>
        /// <param name="path">The path of the RiotArchiveFile to get.</param>
        /// <param name="file">When this method returns, if the key is found, contains the RiotArchiveFile associated with the specified key; otherwise, null.</param>
        /// <returns><c>true</c> if this RiotArchive contains an element with the specified key; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(string path, out RiotArchiveFile file)
        {
            RiotArchiveFile outFile = null;
            var result = Archives.Any(a => a.TryGetValue(path, out outFile));
            file = outFile;
            return result;
        }
    }
}
