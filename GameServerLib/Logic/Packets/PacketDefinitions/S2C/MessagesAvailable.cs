using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class MessagesAvailable : BasePacket
    {
        public MessagesAvailable(Game game, int messagesAvailable)
            : base(game, PacketCmd.PKT_S2C_MESSAGES_AVAILABLE)
        {
            // The following structure might be incomplete or wrong
            Write(messagesAvailable);
        }
    }
}