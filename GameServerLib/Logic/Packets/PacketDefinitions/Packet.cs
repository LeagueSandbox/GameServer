using System.IO;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class Packet
    {
        protected readonly Game Game = Program.ResolveDependency<Game>();

        private MemoryStream memStream;
        protected BinaryWriter buffer;
        public BinaryWriter getBuffer()
        {
            return buffer;
        }

        public Packet(PacketCmd cmd = PacketCmd.PKT_KeyCheck)
        {
            memStream = new MemoryStream();
            buffer = new BinaryWriter(memStream);

            buffer.Write((byte)cmd);
        }

        internal byte[] GetBytes()
        {
            return memStream.ToArray();
        }
    }
}
