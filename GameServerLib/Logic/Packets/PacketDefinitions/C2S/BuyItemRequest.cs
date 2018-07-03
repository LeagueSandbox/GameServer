using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class BuyItemRequest
    {
        private PacketCmd _cmd;
        private int _netId;
        public int Id;
        public BuyItemRequest(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                _cmd = (PacketCmd)reader.ReadByte();
                _netId = reader.ReadInt32();
                Id = reader.ReadInt32();
            }
        }
    }
}