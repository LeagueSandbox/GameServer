using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SpellEmpower : BasePacket
    {
        public SpellEmpower(AttackableUnit unit, byte slot, byte empowerLevel)
            : base(PacketCmd.PKT_S2_C_SPELL_EMPOWER, unit.NetId)
        {
            _buffer.Write(slot);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x06); // Unknown
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);
            _buffer.Write(empowerLevel); // 0 - normal, 1 - empowered (for Rengar)
        }
    }
}