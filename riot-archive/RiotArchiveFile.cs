using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace freedompeace.RiotArchive
{
    /// <summary>
    /// Provides access to information about a file in a RiotArchive.
    /// </summary>
    public class RiotArchiveFile
    {
        public string Name { get { return Path.GetFileName(path); } }
        public string FullName { get { return path; } }

        readonly string archiveFile;
        readonly string path;
        readonly int dataOffset;
        readonly int dataLength;

        /// <summary>
        /// Returns the Riot Hash for this file name.
        /// </summary>
        public uint Hash { get { return RiotArchiveHash.GetHash(this.path); } }

        // Writing to a RiotArchiveFile isn't supported yet. For the moment we're using it internally to
        // check that our hashing algorithm matches Riot's.
        /// <summary>
        /// Constructs a new RiotArchiveFile with the specified file path
        /// </summary>
        /// <param name="virtualFilePath"></param>
        internal RiotArchiveFile(string virtualFilePath)
        {
            this.path = virtualFilePath;
        }

        internal RiotArchiveFile(string archiveFile, string path, int dataOffset, int dataLength)
        {
            this.archiveFile = archiveFile;
            this.path = path;
            this.dataOffset = dataOffset;
            this.dataLength = dataLength;
        }

        /// <summary>
        /// Gets a stream for reading the contents of this file.
        /// </summary>
        public Stream GetStream()
        {
            var stream = new RiotArchiveFileStream(this.archiveFile, this.dataOffset, this.dataLength);
            stream.Open();
            return stream;
        }

        /// <summary>
        /// Gets an <see cref="IEnumerable{byte}"/> for reading the contents of this file.
        /// </summary>
        public IEnumerable<byte> GetEnumerable()
        {
            return GetEnumerableInner(GetStream());
        }

        IEnumerable<byte> GetEnumerableInner(Stream stream)
        {
            while (true)
            {
                var b = stream.ReadByte();
                if (b == -1)
                    yield break;
                yield return (byte)b;
            }
        }
    }
}
