using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class BuyItemResponse : BasePacket
    {
        public BuyItemResponse(Game game, AttackableUnit actor, Item item, byte unk = 0x29)
            : base(game, PacketCmd.PKT_S2C_BUY_ITEM_ANS, actor.NetId)
        {
            Write((int)item.ItemType.ItemId);
            Write((byte)actor.Inventory.GetItemSlot(item));
            Write((byte)item.StackSize);
            Write((byte)0); //unk or stacks => short
            Write((byte)unk); //unk (turret 0x01 and champions 0x29)
        }
    }
}