using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class FloatingTextWithValue : BasePacket
    {
        public FloatingTextWithValue(AttackableUnit u, int value, string text)
            : base(PacketCmd.PKT_S2C_FLOATING_TEXT_WITH_VALUE)
        {
            Write(u.NetId);
            Write(15); // Unk
            Write(value); // Example -3
            Write(Encoding.Default.GetBytes(text));
            Write((byte)0x00);
        }
    }
}