using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SwapItemsResponse : BasePacket
    {
        public SwapItemsResponse(Champion c, byte slotFrom, byte slotTo)
            : base(PacketCmd.PKT_S2C_SwapItems, c.NetId)
        {
            buffer.Write((byte)slotFrom);
            buffer.Write((byte)slotTo);
        }
    }
}