using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class BuyItemResponse : BasePacket
    {
        public BuyItemResponse(AttackableUnit actor, Item item, byte unk = 0x29)
            : base(PacketCmd.PKT_S2_C_BUY_ITEM_ANS, actor.NetId)
        {
            _buffer.Write((int)item.ItemType.ItemId);
            _buffer.Write((byte)actor.Inventory.GetItemSlot(item));
            _buffer.Write((byte)item.StackSize);
            _buffer.Write((byte)0); //unk or stacks => short
            _buffer.Write((byte)unk); //unk (turret 0x01 and champions 0x29)
        }
    }
}