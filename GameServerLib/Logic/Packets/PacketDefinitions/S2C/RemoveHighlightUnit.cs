using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class RemoveHighlightUnit : BasePacket
    {
        public RemoveHighlightUnit(uint netId)
            : base(PacketCmd.PKT_S2C_RemoveHighlightUnit)
        {
            // The following structure might be incomplete or wrong
            buffer.Write((uint)netId);
        }
    }
}