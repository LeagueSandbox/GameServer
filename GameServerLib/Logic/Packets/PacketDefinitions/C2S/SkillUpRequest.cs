using System.IO;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S
{
    public class SkillUpRequest : ClientPacketBase
    {
        public byte Skill { get; private set; }

        public SkillUpRequest(byte[] data) : base(data)
        {

        }

        protected override void ParseInternal(BinaryReader reader)
        {
            Skill = reader.ReadByte();
        }
    }
}