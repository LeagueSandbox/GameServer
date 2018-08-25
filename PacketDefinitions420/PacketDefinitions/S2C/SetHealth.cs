using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SetHealth : BasePacket
    {
        public SetHealth(IAttackableUnit u)
            : base(PacketCmd.PKT_S2C_SET_HEALTH, u.NetId)
        {
            Write((short)0x0000); // unk,maybe flags for physical/magical/true dmg
            Write(u.Stats.HealthPoints.Total);
            Write(u.Stats.CurrentHealth);
        }

        public SetHealth(uint itemHash)
            : base(PacketCmd.PKT_S2C_SET_HEALTH, itemHash)
        {
            Write((short)0);
        }

    }
}