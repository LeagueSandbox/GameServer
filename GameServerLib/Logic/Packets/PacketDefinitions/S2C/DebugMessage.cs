using System.Text;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class DebugMessage : BasePacket
    {
        public DebugMessage(Game game, string message)
            : base(game, PacketCmd.PKT_S2C_DEBUG_MESSAGE)
        {
            Write(0);
			WriteConstLengthString(message, 512);
        }
    }
}