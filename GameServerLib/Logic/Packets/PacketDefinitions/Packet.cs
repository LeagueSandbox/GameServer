using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions
{
    public class Packet
    {
        protected readonly Game Game = Program.ResolveDependency<Game>();

        private MemoryStream _memStream;
        protected BinaryWriter _buffer;
        public BinaryWriter GetBuffer()
        {
            return _buffer;
        }

        public Packet(PacketCmd cmd = PacketCmd.PKT_KEY_CHECK)
        {
            _memStream = new MemoryStream();
            _buffer = new BinaryWriter(_memStream);

            _buffer.Write((byte)cmd);
        }

        internal byte[] GetBytes()
        {
            return _memStream.ToArray();
        }
    }
}
