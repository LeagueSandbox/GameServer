using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SetHealth : BasePacket
    {
        public SetHealth(AttackableUnit u) 
            : base(PacketCmd.PKT_S2C_SetHealth, u.NetId)
        {
            buffer.Write((short)0x0000); // unk,maybe flags for physical/magical/true dmg
            buffer.Write((float)u.Stats.HealthPoints.Total);
            buffer.Write((float)u.Stats.CurrentHealth);
        }

        public SetHealth(uint itemHash) 
            : base(PacketCmd.PKT_S2C_SetHealth, itemHash)
        {
            buffer.Write((short)0);
        }

    }
}