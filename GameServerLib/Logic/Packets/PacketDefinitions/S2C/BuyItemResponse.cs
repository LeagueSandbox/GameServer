using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class BuyItemResponse : BasePacket
    {
        public BuyItemResponse(AttackableUnit actor, Item item, byte unk = 0x29)
            : base(PacketCmd.PKT_S2C_BuyItemAns, actor.NetId)
        {
            buffer.Write((int)item.ItemType.ItemId);
            buffer.Write((byte)actor.Inventory.GetItemSlot(item));
            buffer.Write((byte)item.StackSize);
            buffer.Write((byte)0); //unk or stacks => short
            buffer.Write((byte)unk); //unk (turret 0x01 and champions 0x29)
        }
    }
}