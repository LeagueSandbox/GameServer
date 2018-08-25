using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class MessagesAvailable : BasePacket
    {
        public MessagesAvailable(int messagesAvailable)
            : base(PacketCmd.PKT_S2C_MESSAGES_AVAILABLE)
        {
            // The following structure might be incomplete or wrong
            Write(messagesAvailable);
        }
    }
}