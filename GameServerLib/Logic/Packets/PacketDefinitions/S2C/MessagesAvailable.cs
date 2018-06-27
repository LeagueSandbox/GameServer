using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class MessagesAvailable : BasePacket
    {
        public MessagesAvailable(int messagesAvailable)
            : base(PacketCmd.PKT_S2_C_MESSAGES_AVAILABLE)
        {
            // The following structure might be incomplete or wrong
            _buffer.Write(messagesAvailable);
        }
    }
}