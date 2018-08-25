using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SwapItemsResponse : BasePacket
    {
        public SwapItemsResponse(IChampion c, byte slotFrom, byte slotTo)
            : base(PacketCmd.PKT_S2C_SWAP_ITEMS, c.NetId)
        {
            Write(slotFrom);
            Write(slotTo);
        }
    }
}