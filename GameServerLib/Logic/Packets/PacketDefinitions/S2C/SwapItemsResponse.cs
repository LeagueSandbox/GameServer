using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class SwapItemsResponse : BasePacket
    {
        public SwapItemsResponse(Champion c, byte slotFrom, byte slotTo)
            : base(PacketCmd.PKT_S2_C_SWAP_ITEMS, c.NetId)
        {
            _buffer.Write(slotFrom);
            _buffer.Write(slotTo);
        }
    }
}