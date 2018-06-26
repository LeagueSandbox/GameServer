using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChangeSpell : BasePacket
    {
        public ChangeSpell(AttackableUnit unit, int slot, string spell)
            : base(PacketCmd.PKT_S2_C_CHANGE_SPELL, unit.NetId)
        {
            _buffer.Write((byte)slot);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x02);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);
            _buffer.Write((byte)0x00);
            _buffer.Write(Encoding.Default.GetBytes(spell));
            _buffer.Write((byte)0x00);
        }
    }
}