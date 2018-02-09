using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ChangeSpell : BasePacket
    {
        public ChangeSpell(AttackableUnit unit, int slot, string spell)
            : base(PacketCmd.PKT_S2C_ChangeSpell, unit.NetId)
        {
            buffer.Write((byte)slot);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x02);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write(Encoding.Default.GetBytes(spell));
            buffer.Write((byte)0x00);
        }
    }
}