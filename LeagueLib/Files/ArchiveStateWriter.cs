using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace LeagueLib.Files
{
    public class ArchiveStateWriter
    {
        MemoryStream _stream;
        BinaryWriter _writer;

        ArchiveState[] _states;

        public ArchiveStateWriter() { }

        public void WriteArchiveStates(List<ArchiveState> states, string filepath)
        {
            File.WriteAllBytes(filepath, WriteArchiveStates(states.ToArray()));
        }

        public void WriteArchiveStates(ArchiveState[] states, string filepath)
        {
            File.WriteAllBytes(filepath, WriteArchiveStates(states));
        }

        public byte[] WriteArchiveStates(ArchiveState[] states)
        {
            _stream = new MemoryStream();
            _writer = new BinaryWriter(_stream);
            _states = states;

            SerializeArchiveStates();

            return _stream.ToArray();
        }

        private void SerializeArchiveStates()
        {
            _writer.Write((uint)_states.Length);
            for (var i = 0U; i < _states.Length; i++)
                SerializeArchiveState(_states[i]);
        }

        private void SerializeArchiveState(ArchiveState state)
        {
            SerializeString(state.ArchivePath);
            _writer.Write(state.OriginalLength);

            var originalStates = state.OriginalValues.Values.ToArray();

            _writer.Write((uint)originalStates.Length);
            for (var i = 0U; i < originalStates.Length; i++)
                SerializeArchiveFileInfo(originalStates[i]);
        }

        private void SerializeArchiveFileInfo(ArchiveFileInfo info)
        {
            SerializeString(info.Path);
            _writer.Write(info.DataLength);
            _writer.Write(info.DataOffset);
        }

        private void SerializeString(string value)
        {
            var buffer = ASCIIEncoding.ASCII.GetBytes(value);
            _writer.Write((uint)buffer.Length);
            _writer.Write(buffer, 0, buffer.Length);
        }
    }
}
