using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class SetItemStacks2 : BasePacket
    {
        public SetItemStacks2(IAttackableUnit unit, byte slot, byte stack)
            : base(PacketCmd.PKT_S2C_SET_ITEM_STACKS2, unit.NetId)
        {
            Write(slot);
            Write(stack); // Needs more research
        }
    }
}