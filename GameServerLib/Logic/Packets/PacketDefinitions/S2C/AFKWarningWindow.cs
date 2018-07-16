using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AfkWarningWindow : Packet
    {
        public AfkWarningWindow(Game game)
            : base(game, PacketCmd.PKT_S2C_AFK_WARNING_WINDOW)
        {
            // The following structure might be incomplete or wrong
        }
    }
}