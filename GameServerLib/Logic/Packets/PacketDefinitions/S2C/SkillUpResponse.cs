using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SkillUpResponse : BasePacket
    {
        public SkillUpResponse(uint netId, byte skill, byte level, byte pointsLeft)
            : base(PacketCmd.PKT_S2C_SKILL_UP, netId)
        {
            _buffer.Write(skill);
            _buffer.Write(level);
            _buffer.Write(pointsLeft);
        }
    }
}