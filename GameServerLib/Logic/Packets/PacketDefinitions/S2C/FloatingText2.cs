using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class FloatingText2 : BasePacket
    {
        public FloatingText2(AttackableUnit u, string text, byte type, int unk) 
            : base(PacketCmd.PKT_S2C_FLOATING_TEXT, u.NetId)
        {
            Fill(0, 10);
            Write(type); // From 0x00 to 0x1B, 0x1C shows nothing and 0x1D bugsplats
            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)unk);
            Write((byte)0x00);
            Write((byte)0x00);
            Write((byte)0x00);
            Write(text);
            Write((byte)0x00);
        }
    }
}