using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LeagueLib.Files
{
    public class ArchiveStateReader
    {
        private ArchiveState[] _states;

        private MemoryStream _stream;
        private BinaryReader _reader;

        public ArchiveStateReader() { }

        public ArchiveState[] ReadArchiveStates(string filepath)
        {
            return ReadArchiveStates(File.ReadAllBytes(filepath));
        }

        public ArchiveState[] ReadArchiveStates(byte[] data)
        {
            _stream = new MemoryStream(data);
            _stream.Seek(0, SeekOrigin.Begin);
            _reader = new BinaryReader(_stream);

            DeserializeArchiveStates();

            return _states;
        }

        private void DeserializeArchiveStates()
        {
            _states = new ArchiveState[_reader.ReadUInt32()];
            for (var i = 0U; i < _states.Length; i++)
                _states[i] = DeserializeArchiveState();
        }

        private ArchiveState DeserializeArchiveState()
        {
            var result = new ArchiveState();
            result.ArchivePath = DeserializeString();
            result.OriginalLength = _reader.ReadInt64();
            result.OriginalValues = new Dictionary<string, ArchiveFileInfo>();

            var count = _reader.ReadInt32();
            for (var i = 0U; i < count; i++)
            {
                var info = DeserializeArchiveFileInfo();
                result.OriginalValues[info.Path] = info;
            }

            return result;
        }

        private ArchiveFileInfo DeserializeArchiveFileInfo()
        {
            var result = new ArchiveFileInfo();
            result.Path = DeserializeString();
            result.DataLength = _reader.ReadUInt32();
            result.DataOffset = _reader.ReadUInt32();
            return result;
        }

        private string DeserializeString()
        {
            var buffer = new byte[_reader.ReadUInt32()];
            _reader.Read(buffer, 0, buffer.Length);
            return ASCIIEncoding.ASCII.GetString(buffer);
        }
    }
}
