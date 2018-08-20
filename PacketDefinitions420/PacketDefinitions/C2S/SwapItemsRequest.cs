using System.IO;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.C2S
{
    public class SwapItemsRequest
    {
        public PacketCmd Cmd;
        public int NetId;
        public byte SlotFrom;
        public byte SlotTo;

        public SwapItemsRequest(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                Cmd = (PacketCmd)reader.ReadByte();
                NetId = reader.ReadInt32();
                SlotFrom = reader.ReadByte();
                SlotTo = reader.ReadByte();
            }
        }
    }
}