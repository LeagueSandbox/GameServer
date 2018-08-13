using System.IO;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.C2S
{
    public class SellItem
    {
        public PacketCmd Cmd;
        public int NetId;
        public byte SlotId;

        public SellItem(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                Cmd = (PacketCmd)reader.ReadByte();
                NetId = reader.ReadInt32();
                SlotId = reader.ReadByte();
            }
        }
    }
}