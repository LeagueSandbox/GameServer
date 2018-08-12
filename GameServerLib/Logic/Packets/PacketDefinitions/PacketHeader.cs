using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class PacketHeader
    {
        public PacketHeader()
        {
            netId = 0;
        }

        public PacketHeader(byte[] bytes)
        {
            var reader = new BinaryReader(new MemoryStream(bytes));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            reader.Close();
        }

        public PacketCmd cmd;
        public int netId;
    }
}