using GameServerCore.Logic.Domain;
using GameServerCore.Logic.Domain.GameObjects;
using GameServerCore.Packets.Enums;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class BuyItemResponse : BasePacket
    {
        public BuyItemResponse(IAttackableUnit actor, IItem item, byte unk = 0x29)
            : base(PacketCmd.PKT_S2C_BUY_ITEM_ANS, actor.NetId)
        {
            Write((int)item.ItemType.ItemId);
            Write((byte)actor.Inventory.GetItemSlot(item));
            Write((byte)item.StackSize);
            Write((byte)0); //unk or stacks => short
            Write((byte)unk); //unk (turret 0x01 and champions 0x29)
        }
    }
}