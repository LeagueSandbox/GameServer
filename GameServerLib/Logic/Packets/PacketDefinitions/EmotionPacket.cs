using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class EmotionPacket : BasePacket
    {
        public PacketCmd cmd;
        public uint netId;
        public byte id;

        public EmotionPacket(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadUInt32();
            id = reader.ReadByte();
        }

        public EmotionPacket(byte id, uint netId) : base(PacketCmd.PKT_S2C_Emotion, netId)
        {
            buffer.Write((byte)id);
        }
    }
}