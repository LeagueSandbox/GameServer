using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class BuyItemRequest
    {
        PacketCmd cmd;
        int netId;
        public int id;
        public BuyItemRequest(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            id = reader.ReadInt32();
        }
    }
}