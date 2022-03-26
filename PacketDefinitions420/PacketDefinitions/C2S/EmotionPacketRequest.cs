using System.IO;
using GameServerCore.Enums;

namespace PacketDefinitions420.PacketDefinitions.C2S
{
    public class EmotionPacketRequest
    {
        public PacketCmd Cmd;
        public uint NetId;
        public EmoteID Id;

        public EmotionPacketRequest(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                Cmd = (PacketCmd)reader.ReadByte();
                NetId = reader.ReadUInt32();
                Id = (EmoteID)reader.ReadByte();
            }
        }
    }
}