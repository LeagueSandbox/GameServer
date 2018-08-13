using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChangeSpell : BasePacket
    {
        public ChangeSpell(AttackableUnit unit, int slot, string spell)
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