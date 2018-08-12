using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class EmotionPacketRequest
    {
        public PacketCmd cmd;
        public uint netId;
        public byte id;

        public EmotionPacketRequest(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadUInt32();
            id = reader.ReadByte();
        }
    }
}