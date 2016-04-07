using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LeagueLib.Hashes;

namespace LeagueLib.Files
{
    public class ArchiveReader
    {
        MemoryStream _stream;
        BinaryReader _reader;

        uint _fileListOffset;
        uint _pathListOffset;

        ArchiveFileInfoListEntry[] _fileList;
        ArchivePathInfoListEntry[] _pathList;

        Archive _archive;

        public ArchiveReader() { }

        public Archive ReadArchive(string filepath)
        {
            return ReadArchive(File.ReadAllBytes(filepath), filepath);
        }

        public Archive ReadArchive(byte[] data, string filepath)
        {
            _stream = new MemoryStream(data);
            _reader = new BinaryReader(_stream);

            _archive = new Archive();
            _archive.FilePath = filepath;

            DeserializeArchive();

            if (File.Exists(_archive.DataFilePath))
            {
                var fs = new FileStream(_archive.DataFilePath, FileMode.Open, FileAccess.Read);
                _archive.DataLength = fs.Length;
                fs.Close();
            }

            return _archive;
        }

        public byte[] ReadData(Archive archive, long offset, long length)
        {
            var result = new byte[length];
            var stream = new FileStream(archive.DataFilePath, FileMode.Open, FileAccess.Read);
            stream.Seek(offset, SeekOrigin.Begin);
            stream.Read(result, 0, result.Length);
            stream.Close();
            return result;
        }

        private void DeserializeArchive()
        {
            DeserializeHeader();
            DeserializeFileInfoList();
            DeserializePathInfoList();
            ProcessData();
        }

        private void DeserializeHeader()
        {
            // Skip the magic number (first 4 bytes), no need to read it
            _stream.Seek(4, SeekOrigin.Begin);

            _archive.ArchiveVersion = _reader.ReadUInt32();
            _archive.ArchiveIndex = _reader.ReadUInt32();
            _fileListOffset = _reader.ReadUInt32();
            _pathListOffset = _reader.ReadUInt32();
        }

        private void DeserializeFileInfoList()
        {
            _stream.Seek(_fileListOffset, SeekOrigin.Begin);

            _fileList = new ArchiveFileInfoListEntry[_reader.ReadUInt32()];
            for (uint i = 0; i < _fileList.Length; i++)
                _fileList[i] = DeserializeFileInfoListEntry();
        }

        private ArchiveFileInfoListEntry DeserializeFileInfoListEntry()
        {
            var result = new ArchiveFileInfoListEntry();
            result.PathHash = _reader.ReadUInt32();
            result.DataOffset = _reader.ReadUInt32();
            result.DataLength = _reader.ReadUInt32();
            result.PathIndex = _reader.ReadUInt32();
            return result;
        }

        private void DeserializePathInfoList()
        {
            // We don't care about how many bytes the path list has, so skip first 4 bytes.
            _stream.Seek(_pathListOffset + 4, SeekOrigin.Begin);

            _pathList = new ArchivePathInfoListEntry[_reader.ReadUInt32()];
            for (uint i = 0; i < _pathList.Length; i++)
                _pathList[i] = DeserializePathInfoListEntry();
        }

        private ArchivePathInfoListEntry DeserializePathInfoListEntry()
        {
            var result = new ArchivePathInfoListEntry();
            result.PathOffset = _reader.ReadUInt32();
            result.PathLength = _reader.ReadUInt32();
            return result;
        }

        private void ProcessData()
        {
            _archive.Files = new Dictionary<string, ArchiveFileInfo>();

            for(uint i = 0; i < _fileList.Length; i++)
            {
                
                var result = new ArchiveFileInfo();
                result.Path = ReadString(_pathListOffset + _pathList[_fileList[i].PathIndex].PathOffset, _pathList[_fileList[i].PathIndex].PathLength - 1);

                if(_fileList[i].PathHash != HashFunctions.LeagueHash(result.Path))
                {
                    Console.WriteLine("Invalid hash for a path: {0}", result.Path);
                    continue;
                }

                result.DataOffset = _fileList[i].DataOffset;
                result.DataLength = _fileList[i].DataLength;

                _archive.Files.Add(result.Path, result);
            }
        }

        private string ReadString(uint offset, uint length)
        {
            var buffer = new byte[length];
            _stream.Seek(offset, SeekOrigin.Begin);
            _stream.Read(buffer, 0, buffer.Length);
            return ASCIIEncoding.ASCII.GetString(buffer, 0, buffer.Length);
        }
    }

    public class ArchiveFileInfoListEntry
    {
        public uint PathHash { get; set; }
        public uint DataOffset { get; set; }
        public uint DataLength { get; set; }
        public uint PathIndex { get; set; }
    }

    public class ArchivePathInfoListEntry
    {
        public uint PathOffset { get; set; }
        public uint PathLength { get; set; }
    }
}
