using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class RemoveMessageBoxRight : BasePacket
    {
        public RemoveMessageBoxRight(Game game)
            : base(PacketCmd.PKT_S2C_REMOVE_MESSAGE_BOX_RIGHT)
        {
            // The following structure might be incomplete or wrong
        }
    }
}