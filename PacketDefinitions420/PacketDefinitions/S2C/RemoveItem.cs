using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class RemoveItem : BasePacket
    {
        public RemoveItem(IAttackableUnit u, byte slot, short remaining)
            : base(PacketCmd.PKT_S2C_REMOVE_ITEM, u.NetId)
        {
            Write(slot);
            Write(remaining);
        }
    }
}