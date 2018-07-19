using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SkillUpResponse : BasePacket
    {
        public SkillUpResponse(Game game, uint netId, byte skill, byte level, byte pointsLeft)
            : base(game, PacketCmd.PKT_S2C_SKILL_UP, netId)
        {
            Write(skill);
            Write(level);
            Write(pointsLeft);
        }
    }
}