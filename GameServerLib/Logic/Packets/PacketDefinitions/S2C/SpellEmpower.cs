using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpellEmpower : BasePacket
    {
        public SpellEmpower(AttackableUnit unit, byte slot, byte empowerLevel)
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