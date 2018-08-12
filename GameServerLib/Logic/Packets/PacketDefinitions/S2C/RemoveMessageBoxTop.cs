using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class RemoveMessageBoxTop : BasePacket
    {
        public RemoveMessageBoxTop(Game game)
            : base(PacketCmd.PKT_S2C_REMOVE_MESSAGE_BOX_TOP)
        {
            // The following structure might be incomplete or wrong
        }
    }
}