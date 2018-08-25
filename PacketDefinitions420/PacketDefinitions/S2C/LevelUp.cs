using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class LevelUp : BasePacket
    {
        public LevelUp(IChampion c)
            : base(PacketCmd.PKT_S2C_LEVEL_UP, c.NetId)
        {
            Write(c.Stats.Level);
            Write(c.SkillPoints);
        }
    }
}