using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class RemoveMessageBoxTop : BasePacket
    {
        public RemoveMessageBoxTop()
            : base(PacketCmd.PKT_S2C_REMOVE_MESSAGE_BOX_TOP)
        {
            // The following structure might be incomplete or wrong
        }
    }
}