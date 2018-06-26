using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class FloatingTextWithValue : BasePacket
    {
        public FloatingTextWithValue(AttackableUnit u, int value, string text)
            : base(PacketCmd.PKT_S2_C_FLOATING_TEXT_WITH_VALUE)
        {
            _buffer.Write(u.NetId);
            _buffer.Write((int)15); // Unk
            _buffer.Write(value); // Example -3
            _buffer.Write(Encoding.Default.GetBytes(text));
            _buffer.Write((byte)0x00);
        }
    }
}