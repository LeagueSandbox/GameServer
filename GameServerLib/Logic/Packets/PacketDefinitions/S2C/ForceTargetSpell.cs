using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ForceTargetSpell : BasePacket
    {
        public ForceTargetSpell(AttackableUnit u, byte slot, float time)
            : base(PacketCmd.PKT_S2C_FORCE_TARGET_SPELL, u.NetId)
        {
            Write(slot);
            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x00);
            Write(time);
        }
    }
}