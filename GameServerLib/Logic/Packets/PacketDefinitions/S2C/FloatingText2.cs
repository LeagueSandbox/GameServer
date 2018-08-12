using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class FloatingText2 : BasePacket
    {
        public FloatingText2(AttackableUnit u, string text, byte type, int unk) : base(PacketCmd.PKT_S2C_FloatingText, u.NetId)
        {
            buffer.fill(0, 10);
            buffer.Write((byte)type); // From 0x00 to 0x1B, 0x1C shows nothing and 0x1D bugsplats
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)unk);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write((byte)0x00);
            buffer.Write(Encoding.Default.GetBytes(text));
            buffer.Write((byte)0x00);
        }
    }
}