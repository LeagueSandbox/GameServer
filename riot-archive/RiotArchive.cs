using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using complete.IO;

namespace freedompeace.RiotArchive
{
    /// <summary>
    /// Provides read access to a Riot Archive Files, used in the League of Legends game.
    /// </summary>
    /// <remarks>
    /// <para>Writing Riot Archive Files has not been implemented and is not supported yet.</para>
    /// <para>To read a Riot Archive File from disk, create a new <c>RiotArchive</c> instance
    /// and call the <c>Open</c> method.</para>
    /// <para>Riot Archive Files (or "raf files") are a pair of *.raf and *.raf.dat files. The
    /// *.dat file contains the raw data as a big blob, and the *.raf.dat file contains the
    /// metadata required to interpret the pair.</para>
    /// <para>The current implementation of RiotArchive does not support files that involve file
    /// sizes over 2GB. This includes everything, from the archive itself to archived files to
    /// internal strings.</para>
    /// </remarks>
    public class RiotArchive
    {
        const uint kMagic = 0x18BE0EF0;
        const uint kVersion = 1;

        // Gets the full path of Riot archive file.
        public string FullName { get; private set; }
        public uint ManagerIndex { get; private set; }
        public IEnumerable<RiotArchiveFile> Files { get { return files.Values; } }

        // Dictionary<(string)lowerCasePath, (RiotArchiveFile)file>
        Dictionary<string, RiotArchiveFile> files;
        bool initialised;

        /// <summary>
        /// Initialises a new instance of the RiotArchive class with the specified path. To read the
        /// file on disk, the RiotArchive::Open() method must additionally be called.
        /// </summary>
        /// <param name="path">The path of the RiotArchive with the .raf file extension</param>
        public RiotArchive(string path)
        {
            this.FullName = path;
        }

        /// <summary>
        /// Creates a RiotArchive instance from a file
        /// </summary>
        /// <param name="path">The path of the RiotArchive with the .raf file extension</param>
        /// <returns>A RiotArchive instance representing the file</returns>
        public static RiotArchive FromFile(string path)
        {
            var archive = new RiotArchive(path);
            archive.Open();
            return archive;
        }
        
        /// <summary>
        /// Opens and loads the Riot Archive File from the disk.
        /// </summary>
        public void Open()
        {
            if (initialised)
                return;
            initialised = true;

            using (var fileStream = new FileStream(this.FullName, FileMode.Open, FileAccess.Read))
            {
                var buffer = new byte[fileStream.Length];
                fileStream.Read(buffer, 0, (int)fileStream.Length);
                var reader = new BinaryReader(new MemoryStream(buffer));

                var header = reader.ReadStruct<RiotArchiveHeader>();
                if (header.Magic != kMagic || header.Version != kVersion)
                    throw new FileFormatIncorrectException();

                this.ManagerIndex = header.ManagerIndex;
                var pathList = GetPathList(reader, header).ToArray();
                var files = GetFiles(reader, header, pathList).ToArray();
                this.files = files.ToDictionary(x => x.FullName.ToLowerInvariant(), x => x);
            }
        }

        IEnumerable<string> GetPathList(BinaryReader reader, RiotArchiveHeader header)
        {
            var stream = reader.BaseStream;
            stream.Seek(header.PathListOffset, SeekOrigin.Begin);

            var pathListInfo = reader.ReadStruct<RiotPathListHeader>();
            var stringOffsets = GetStringOffsets(reader, (int)pathListInfo.Length).ToArray();
            foreach (var offset in stringOffsets)
            {
                stream.Seek(header.PathListOffset + offset.Offset, SeekOrigin.Begin);
                yield return BinaryReaderTools.GetStaticLengthString(reader, (int)offset.Length);
            }
        }

        IEnumerable<RiotPathListItem> GetStringOffsets(BinaryReader reader, int stringCount)
        {
            for (int i = 0; i < stringCount; i++)
                yield return reader.ReadStruct<RiotPathListItem>();
        }

        IEnumerable<RiotArchiveFile> GetFiles(BinaryReader reader, RiotArchiveHeader header, string[] paths)
        {
            var stream = reader.BaseStream;
            stream.Seek(header.FilesOffset, SeekOrigin.Begin);

            var fileListInfo = reader.ReadStruct<RiotFileListHeader>();
            for (int i = 0; i < fileListInfo.Length; i++)
            {
                var fileInfo = reader.ReadStruct<RiotFileListEntry>();
                yield return new RiotArchiveFile(this.FullName, paths[fileInfo.PathListIndex], (int)fileInfo.DataOffset, (int)fileInfo.DataLength);

#if DEBUG
                Debug.Assert(new RiotArchiveFile(paths[fileInfo.PathListIndex]).Hash == fileInfo.Hash);
#endif
            }
        }

        /// <summary>
        /// Returns an enumerable collection of file names that match a search pattern in a specified path.
        /// </summary>
        /// <param name="searchPattern">The regular expression to match against the names of directories in path.</param>
        /// <returns>An enumerable collection of the full names (including paths) for the files in the directory specified by <paramref name="path"/> and that match the specified search pattern.</returns>
        /// <remarks>The order in which files are returned is unspecified (unordered).</remarks>
        public IEnumerable<RiotArchiveFile> GetFiles(string searchPattern)
        {
            if (this.files == null)
                return Enumerable.Empty<RiotArchiveFile>();

            return new RiotArchiveSearchEnumerable(this.files.Values, searchPattern);
        }

        /// <summary>
        /// Determines whether the given path refers to an existing file in the archive.
        /// </summary>
        /// <param name="path">The path to test.</param>
        /// <returns><value>true</value> if path refers to an existing directory; otherwise, <value>false</value></returns>
        public bool Exists(string path)
        {
            return files.ContainsKey(path.ToLowerInvariant());
        }

        /// <summary>
        /// Gets the RiotArhiveFile associated with the specified path.
        /// </summary>
        /// <param name="path">The path of the RiotArchiveFile to get.</param>
        /// <returns></returns>
        public RiotArchiveFile this[string path]
        {
            get { return files[path.ToLowerInvariant()]; }
        }

        /// <summary>
        /// Gets the RiotArhiveFile associated with the specified path.
        /// </summary>
        /// <param name="path">The path of the RiotArchiveFile to get.</param>
        /// <param name="file">When this method returns, if the key is found, contains the RiotArchiveFile associated with the specified key; otherwise, null.</param>
        /// <returns><c>true</c> if this RiotArchive contains an element with the specified key; otherwise, <c>false</c>.</returns>
        public bool TryGetValue(string path, out RiotArchiveFile file)
        {
            return files.TryGetValue(path.ToLowerInvariant(), out file);
        }
    }
}
