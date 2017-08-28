using LeagueSandbox.GameServer.Logic.Content;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets.PacketArgs;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class BuyItemResponse : BasePacket
    {
        public BuyItemResponse(BuyItemResponseArgs args)
            : base(PacketCmd.PKT_S2C_BuyItemAns, args.UnitNetId)
        {
            buffer.Write((int)args.ItemId);
            buffer.Write((byte)args.ItemSlot);
            buffer.Write((byte)args.StackSize);
            buffer.Write((byte)0); //unk or stacks => short
            buffer.Write((byte)args.ItemOwner); //unk (turret 0x01 and champions 0x29)
        }
    }
}