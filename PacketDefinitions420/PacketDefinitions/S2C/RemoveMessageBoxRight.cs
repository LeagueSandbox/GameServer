using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class RemoveMessageBoxRight : BasePacket
    {
        public RemoveMessageBoxRight()
            : base(PacketCmd.PKT_S2C_REMOVE_MESSAGE_BOX_RIGHT)
        {
            // The following structure might be incomplete or wrong
        }
    }
}