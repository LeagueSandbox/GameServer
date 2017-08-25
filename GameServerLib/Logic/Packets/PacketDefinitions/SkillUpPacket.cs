using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class SkillUpPacket : BasePacket
    {
        public PacketCmd cmd;
        public uint netId;
        public byte skill;
        public SkillUpPacket(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            cmd = (PacketCmd)reader.ReadByte();
            netId = reader.ReadUInt32();
            skill = reader.ReadByte();
        }
        public SkillUpPacket(uint netId, byte skill, byte level, byte pointsLeft)
            : base(PacketCmd.PKT_S2C_SkillUp, netId)
        {
            buffer.Write(skill);
            buffer.Write(level);
            buffer.Write(pointsLeft);
        }
    }
}