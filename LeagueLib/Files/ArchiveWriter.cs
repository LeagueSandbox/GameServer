using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using LeagueLib.Hashes;

namespace LeagueLib.Files
{
    public class ArchiveWriter
    {
        private const uint FILE_LIST_OFFSET = 24;
        private const uint FILE_INFO_ENTRY_SIZE = 16;
        private const uint PATH_INFO_ENTRY_SIZE = 8;

        private MemoryStream _stream;
        private BinaryWriter _writer;
        private uint[] _pathOffsets;
        private uint _pathTableLength;

        private Archive _archive;
        private ArchiveFileInfo[] _files;

        public ArchiveWriter() { }

        public void WriteArchive(Archive archive, string filepath)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filepath))) { Directory.CreateDirectory(Path.GetDirectoryName(filepath)); }
            File.WriteAllBytes(filepath, WriteArchive(archive));
        }

        public byte[] WriteArchive(Archive archive)
        {
            _archive = archive;

            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);

            _files = _archive.Files.Values.ToArray();

            SerializeArchive();

            return _stream.ToArray();
        }

        public long WriteData(Archive archive, byte[] data)
        {
            var stream = new FileStream(archive.DataFilePath, FileMode.Open, FileAccess.ReadWrite);
            stream.Seek(0, SeekOrigin.End);
            var offset = stream.Position;
            stream.Write(data, 0, data.Length);
            stream.Flush();
            stream.Close();
            return offset;
        }

        public void SetDataLength(Archive archive, long length)
        {
            var stream = new FileStream(archive.DataFilePath, FileMode.Open, FileAccess.ReadWrite);
            stream.SetLength(length);
        }

        private void SerializeArchive()
        {
            SerializeHeader();
            SerializeFileInfoList();
            SerializePathInfoList();
        }

        private void SerializeHeader()
        {
            _writer.Write((uint)Archive.MAGIC_VALUE);
            _writer.Write(_archive.ArchiveVersion);
            _writer.Write(_archive.ArchiveIndex);
            _writer.Write(FILE_LIST_OFFSET);
            _writer.Write(FILE_LIST_OFFSET + _archive.Files.Count * FILE_INFO_ENTRY_SIZE + 4);
        }

        private void SerializeFileInfoList()
        {
            _writer.Write((uint)_files.Length);
            for (var i = 0U; i < _files.Length; i++)
                SerializeFileInfoListEntry(i);
        }

        private void SerializeFileInfoListEntry(uint id)
        {
            _writer.Write(HashFunctions.LeagueHash(_files[id].Path));
            _writer.Write(_files[id].DataOffset);
            _writer.Write(_files[id].DataLength);
            _writer.Write(id);
        }

        private void SerializePathInfoList()
        {
            SerializePathTable(PATH_INFO_ENTRY_SIZE * _files.Length + 8);
            _writer.Write(_pathTableLength);
            _writer.Write(_files.Length);
            for (var i = 0U; i < _files.Length; i++)
                SerializePathInfoListEntry(i);
        }

        private void SerializePathInfoListEntry(uint id)
        {
            _writer.Write(_pathOffsets[id]);
            _writer.Write((uint)_files[id].Path.Length + 1);
        }

        private void SerializePathTable(long offset)
        {
            var original = _stream.Position;
            _stream.Seek(offset, SeekOrigin.Current);

            _pathOffsets = new uint[_files.Length];
            _pathTableLength = 0;

            for(var i = 0U; i < _files.Length; i++)
            {
                _pathOffsets[i] = (uint)(_stream.Position - original);
                SerializeString(_files[i].Path);
            }

            _stream.Seek(original, SeekOrigin.Begin);
        }

        private void SerializeString(string value)
        {
            var buffer = ASCIIEncoding.ASCII.GetBytes(value);
            _writer.Write(buffer, 0, buffer.Length);
            _writer.Write((byte)0);
            _pathTableLength += (uint)buffer.Length + 1;
        }
    }
}
