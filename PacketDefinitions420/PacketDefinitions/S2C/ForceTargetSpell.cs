using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class ForceTargetSpell : BasePacket
    {
        public ForceTargetSpell(IAttackableUnit u, byte slot, float time)
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