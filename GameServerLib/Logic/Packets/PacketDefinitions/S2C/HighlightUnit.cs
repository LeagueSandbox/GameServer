using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class HighlightUnit : BasePacket
    {
        public HighlightUnit(uint netId) 
            : base(PacketCmd.PKT_S2C_HighlightUnit)
        {
            // The following structure might be incomplete or wrong
            buffer.Write((uint)netId);
        }
    }
}