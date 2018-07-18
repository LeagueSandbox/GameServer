using System.Text;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class FloatingText : BasePacket
    {
        public FloatingText(Game game, AttackableUnit u, string text)
            : base(game, PacketCmd.PKT_S2C_FLOATING_TEXT, u.NetId)
        {
            Write(0); // netid?
            Fill(0, 10);
            Write(0); // netid?
            Write(text);
            Write((byte)0x00);
        }
    }
}