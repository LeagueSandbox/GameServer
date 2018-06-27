using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class FloatingText : BasePacket
    {
        public FloatingText(AttackableUnit u, string text)
            : base(PacketCmd.PKT_S2_C_FLOATING_TEXT, u.NetId)
        {
            _buffer.Write(0); // netid?
            _buffer.Fill(0, 10);
            _buffer.Write(0); // netid?
            _buffer.Write(Encoding.Default.GetBytes(text));
            _buffer.Write((byte)0x00);
        }
    }
}