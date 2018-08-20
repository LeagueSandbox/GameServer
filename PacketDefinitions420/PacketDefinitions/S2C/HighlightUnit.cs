using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class HighlightUnit : BasePacket
    {
        public HighlightUnit(uint netId)
            : base(PacketCmd.PKT_S2C_HIGHLIGHT_UNIT)
        {
            // The following structure might be incomplete or wrong
            Write(netId);
        }
    }
}