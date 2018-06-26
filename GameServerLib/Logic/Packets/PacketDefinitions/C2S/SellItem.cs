using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class SellItem
    {
        public PacketCmd Cmd;
        public int NetId;
        public byte SlotId;

        public SellItem(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            Cmd = (PacketCmd)reader.ReadByte();
            NetId = reader.ReadInt32();
            SlotId = reader.ReadByte();
        }
    }
}