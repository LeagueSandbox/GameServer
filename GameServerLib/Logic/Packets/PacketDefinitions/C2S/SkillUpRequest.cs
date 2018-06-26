using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class SkillUpRequest
    {
        public PacketCmd Cmd;
        public uint NetId;
        public byte Skill;

        public SkillUpRequest(byte[] data)
        {
            var reader = new BinaryReader(new MemoryStream(data));
            Cmd = (PacketCmd)reader.ReadByte();
            NetId = reader.ReadUInt32();
            Skill = reader.ReadByte();
        }
    }
}