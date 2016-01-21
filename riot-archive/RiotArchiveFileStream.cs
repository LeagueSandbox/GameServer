using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using complete.IO;

namespace freedompeace.RiotArchive
{
    /// <summary>
    /// Read-only file stream. This class is NOT thread safe.
    /// </summary>
    public class RiotArchiveFileStream : Stream
    {
        readonly string archiveFile;
        readonly int dataOffset;
        readonly int dataLength;
        bool initialised;

        Stream stream;
        Stream zlibStream;

        static readonly uint ZlibCompressionMethodMask;

        static RiotArchiveFileStream()
        {
            ZlibCompressionMethodMask = Convert.ToUInt32("00001111", 2);
        }

        public RiotArchiveFileStream(string archiveFile, int dataOffset, int dataLength)
        {
            this.archiveFile = archiveFile;
            this.dataOffset = dataOffset;
            this.dataLength = dataLength;
        }

        public void Open()
        {
            if (initialised)
                return;
            initialised = true;

            var fileStream = new FileStream(this.archiveFile + ".dat", FileMode.Open, FileAccess.Read);
            stream = new SubStream(fileStream, dataOffset, dataLength);

            var zlibHeader = new byte[2];
            var bytesRead = stream.Read(zlibHeader, 0, 2);
            if (bytesRead != 2)
                throw new InvalidDataException();

            // reset the stream position after checking header
            stream.Seek(0, SeekOrigin.Begin);

            // See http://tools.ietf.org/html/rfc1950 for zlib file format
            var compressionMethod = zlibHeader[0] & ZlibCompressionMethodMask;
            var compressionInfo = zlibHeader[0] >> 4;

            if (BitConverter.IsLittleEndian)
                Array.Reverse(zlibHeader);

            var hasZlibHeader = compressionMethod == 8 && compressionInfo <= 7 && BitConverter.ToUInt16(zlibHeader, 0) % 31 == 0;
            if (hasZlibHeader)
                zlibStream = new Ionic.Zlib.ZlibStream(stream, Ionic.Zlib.CompressionMode.Decompress);
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                if (zlibStream != null)
                    return zlibStream.Read(buffer, offset, count);
            }
            catch (Ionic.Zlib.ZlibException)
            {
                zlibStream = null;
            }

            return stream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return false; } }
        public override bool CanWrite { get { return false; } }
        public override long Length { get { throw new NotSupportedException(); } }

        public override long Position
        {
            get { throw new NotSupportedException(); }
            set { throw new NotSupportedException(); }
        }

        protected override void Dispose(bool disposing)
        {
            stream.Dispose();
            zlibStream.Dispose();
            base.Dispose(disposing);
        }
    }
}
