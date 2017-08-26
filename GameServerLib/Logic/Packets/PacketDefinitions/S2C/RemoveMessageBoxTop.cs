using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class RemoveMessageBoxTop : BasePacket
    {
        public RemoveMessageBoxTop()
            : base(PacketCmd.PKT_S2C_RemoveMessageBoxTop)
        {
            // The following structure might be incomplete or wrong
        }
    }
}