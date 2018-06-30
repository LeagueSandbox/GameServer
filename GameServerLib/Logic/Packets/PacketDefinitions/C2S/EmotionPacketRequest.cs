using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class EmotionPacketRequest
    {
        public PacketCmd Cmd;
        public uint NetId;
        public byte Id;

        public EmotionPacketRequest(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            Cmd = (PacketCmd)reader.ReadByte();
            NetId = reader.ReadUInt32();
            Id = reader.ReadByte();
        }
    }
}