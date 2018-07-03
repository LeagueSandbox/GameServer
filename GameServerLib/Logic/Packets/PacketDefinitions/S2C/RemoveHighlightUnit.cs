using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class RemoveHighlightUnit : BasePacket
    {
        public RemoveHighlightUnit(uint netId)
            : base(PacketCmd.PKT_S2C_REMOVE_HIGHLIGHT_UNIT)
        {
            // The following structure might be incomplete or wrong
            Write(netId);
        }
    }
}