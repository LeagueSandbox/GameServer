using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AfkWarningWindow : Packet
    {
        public AfkWarningWindow()
            : base(PacketCmd.PKT_S2_C_AFK_WARNING_WINDOW)
        {
            // The following structure might be incomplete or wrong
        }
    }
}