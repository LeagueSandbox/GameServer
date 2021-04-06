using LeaguePackets;
using PacketDefinitions420.PacketDefinitions;
using System.IO;

namespace PacketDefinitions420
{
    public class PacketHeader
    {
        public GamePacketID Cmd;
        public int NetId;

        public PacketHeader()
        {
            NetId = 0;
        }

        public PacketHeader(byte[] bytes)
        {
            var reader = new BinaryReader(new MemoryStream(bytes));
            Cmd = (GamePacketID)reader.ReadByte();
            NetId = reader.ReadInt32();
            reader.Close();
        }
    }
}