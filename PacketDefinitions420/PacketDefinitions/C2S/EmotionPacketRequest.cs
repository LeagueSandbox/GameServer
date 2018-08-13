using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;
using PacketDefinitions420.Enums;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class EmotionPacketRequest
    {
        public PacketCmd Cmd;
        public uint NetId;
        public EmotionType Id;

        public EmotionPacketRequest(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                Cmd = (PacketCmd)reader.ReadByte();
                NetId = reader.ReadUInt32();
                Id = (EmotionType)reader.ReadByte();
            }
        }
    }
}