using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class LevelUp : BasePacket
    {
        public LevelUp(Champion c)
            : base(PacketCmd.PKT_S2C_LEVEL_UP, c.NetId)
        {
            Write(c.Stats.Level);
            Write(c.GetSkillPoints());
        }
    }
}