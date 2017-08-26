using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class SwapItemsRequest
    {
        public PacketCmd cmd;
        public int netId;
        public byte slotFrom;
        public byte slotTo;

        public SwapItemsRequest(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            slotFrom = reader.ReadByte();
            slotTo = reader.ReadByte();
        }
    }
}