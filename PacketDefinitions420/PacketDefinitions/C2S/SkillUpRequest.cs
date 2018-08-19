using System.IO;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.C2S
{
    public class SkillUpRequest
    {
        public PacketCmd Cmd;
        public uint NetId;
        public byte Skill;

        public SkillUpRequest(byte[] data)
        {
            using (var reader = new BinaryReader(new MemoryStream(data)))
            {
                Cmd = (PacketCmd)reader.ReadByte();
                NetId = reader.ReadUInt32();
                Skill = reader.ReadByte();
            }
        }
    }
}