using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class BuyItemReq
    {
        PacketCmd cmd;
        int netId;
        public int id;
        public BuyItemReq(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadInt32();
            id = reader.ReadInt32();
        }
    }
}