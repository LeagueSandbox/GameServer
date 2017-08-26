using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class FloatingText : BasePacket
    {
        public FloatingText(Unit u, string text)
            : base(PacketCmd.PKT_S2C_FloatingText, u.NetId)
        {
            buffer.Write((int)0); // netid?
            buffer.fill(0, 10);
            buffer.Write((int)0); // netid?
            buffer.Write(Encoding.Default.GetBytes(text));
            buffer.Write((byte)0x00);
        }
    }
}