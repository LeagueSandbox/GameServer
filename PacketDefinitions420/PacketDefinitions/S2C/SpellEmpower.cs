using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SpellEmpower : BasePacket
    {
        public SpellEmpower(IAttackableUnit unit, byte slot, byte empowerLevel)
            : base(PacketCmd.PKT_S2C_SPELL_EMPOWER, unit.NetId)
        {
            Write(slot);
            Write((byte)0x00);
            Write((byte)0x06); // Unknown
            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x00);
            Write(empowerLevel); // 0 - normal, 1 - empowered (for Rengar)
        }
    }
}