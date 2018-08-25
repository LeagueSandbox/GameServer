using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class ChangeSpell : BasePacket
    {
        public ChangeSpell(IAttackableUnit unit, int slot, string spell)
            : base(PacketCmd.PKT_S2C_CHANGE_SPELL, unit.NetId)
        {
            Write((byte)slot);
            Write((byte)0x00);
            Write((byte)0x02);
            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x00);
			Write(spell);
            Write((byte)0x00);
        }
    }
}