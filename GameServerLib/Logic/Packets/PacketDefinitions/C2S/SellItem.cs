using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class SellItem
    {
        public PacketCmd cmd;
        public int netId;
        public byte slotId;

        public SellItem(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            slotId = reader.ReadByte();
        }
    }
}