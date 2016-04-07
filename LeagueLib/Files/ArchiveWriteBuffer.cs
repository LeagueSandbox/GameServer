using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LeagueLib.Files
{
    public class ArchiveWriteBuffer
    {
        public List<byte[]> Data { get; private set; }
        public List<ArchiveFileInfo> Metadata { get; private set; }

        private uint _offset;

        public ArchiveWriteBuffer()
        {
            Data = new List<byte[]>();
            Metadata = new List<ArchiveFileInfo>();
            _offset = 0;
        }

        public void WriteData(string filepath, byte[] data)
        {
            var info = new ArchiveFileInfo();
            info.Path = filepath;
            info.DataLength = (uint)data.Length;
            info.DataOffset = _offset;
            Metadata.Add(info);
            Data.Add(data);
            _offset += (uint)data.Length;
        }
    }
}
