using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class FloatingTextWithValue : BasePacket
    {
        public FloatingTextWithValue(AttackableUnit u, int value, string text)
            : base(PacketCmd.PKT_S2C_FloatingTextWithValue)
        {
            buffer.Write(u.NetId);
            buffer.Write((int)15); // Unk
            buffer.Write(value); // Example -3
            buffer.Write(Encoding.Default.GetBytes(text));
            buffer.Write((byte)0x00);
        }
    }
}