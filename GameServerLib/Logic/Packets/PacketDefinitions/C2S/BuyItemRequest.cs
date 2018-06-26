using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class BuyItemRequest
    {
        PacketCmd _cmd;
        int _netId;
        public int Id;
        public BuyItemRequest(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            _cmd = (PacketCmd)reader.ReadByte();
            _netId = reader.ReadInt32();
            Id = reader.ReadInt32();
        }
    }
}