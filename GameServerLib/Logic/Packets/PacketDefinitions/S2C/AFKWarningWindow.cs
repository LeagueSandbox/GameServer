using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class AFKWarningWindow : Packet
    {
        public AFKWarningWindow()
            : base(PacketCmd.PKT_S2C_AFKWarningWindow)
        {
            // The following structure might be incomplete or wrong
        }
    }
}