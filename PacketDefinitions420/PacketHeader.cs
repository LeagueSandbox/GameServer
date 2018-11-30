using PacketDefinitions420.PacketDefinitions;
using System.IO;

namespace PacketDefinitions420
{
    public class PacketHeader
    {
        public PacketHeader()
        {
            NetId = 0;
        }

        public PacketHeader(byte[] bytes)
        {
            var reader = new BinaryReader(new MemoryStream(bytes));
            Cmd = (PacketCmd)reader.ReadByte();
            NetId = reader.ReadInt32();
            reader.Close();
        }

        public PacketCmd Cmd;
        public int NetId;
    }
}