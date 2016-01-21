using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace complete.IO
{
    // Writing not implemented (yet)
    public class SubStream : Stream
    {
        readonly Stream stream;
        readonly long start;
        readonly long length;
        // position in source stream
        long position;
        
        public SubStream(Stream stream, long start, long length)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (start + length > stream.Length)
                throw new ArgumentOutOfRangeException();

            this.stream = stream;
            this.start = start;
            this.position = start;
            this.length = length;
        }

        public override bool CanSeek
        {
            get { return stream.CanSeek; }
        }
        public override bool CanRead
        {
            get { return stream.CanRead; }
        }
        public override bool CanWrite
        {
            get { return false; }
        }
        public override long Length
        {
            get { return length; }
        }
        public override long Position
        {
            get { return position - start; }
            set { stream.Seek(value, SeekOrigin.Begin); }
        }
        public override int Read(byte[] buffer, int offset, int count)
        {
            if (count < 0 || offset < 0)
                throw new ArgumentOutOfRangeException();
            if (count > start + length - position)
                count = (int)(start + length - position);
            stream.Seek(position, SeekOrigin.Begin);
            var bytesRead = stream.Read(buffer, offset, count);
            position += bytesRead;
            return bytesRead;
        }
        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Current:
                    position += offset;
                    break;
                case SeekOrigin.Begin:
                    position = start + offset;
                    break;
                case SeekOrigin.End:
                    position = start + length + offset;
                    break;
            }
            if (position < start)
                position = start;
            if (position > start + length)
                position = start + length;
            return position;
        }
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
        public override void Flush()
        {
            throw new NotSupportedException();
        }
    }
}
