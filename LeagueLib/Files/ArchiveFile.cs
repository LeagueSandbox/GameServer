using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ionic.Zlib;

namespace LeagueLib.Files
{
    public class ArchiveFile
    {
        public byte[] Data { get; set; }
        public uint CompressedSize { get; set; }
        public uint UncompressedSize { get; set; }

        internal ArchiveFile() { }

        public ArchiveFile(byte[] data, uint compressedSize, uint uncompressedSize)
        {
            Data = data;
            CompressedSize = compressedSize;
            UncompressedSize = uncompressedSize;
        }

        public ArchiveFile(byte[] data, bool compress)
        {
            if ((data[0] == 0x78 && (data[1] == 0x01 || data[1] == 0x9C || data[1] == 0xDA)))
            {
                CompressedSize = (uint)data.Length;
                var temp = ZlibStream.UncompressBuffer(data);
                UncompressedSize = (uint)temp.Length;
                if (!compress)
                    data = temp;
            }
            else
            {
                UncompressedSize = (uint)data.Length;
                var temp = ZlibStream.CompressBuffer(data);
                CompressedSize = (uint)temp.Length;
                if (compress)
                    data = temp;
            }

            Data = data;
        }

        public byte[] Uncompress()
        {
            if ((Data[0] == 0x78 && (Data[1] == 0x01 || Data[1] == 0x9C || Data[1] == 0xDA)))
            {
                return ZlibStream.UncompressBuffer(Data);
            }

            return Data;
        }
    }
}
