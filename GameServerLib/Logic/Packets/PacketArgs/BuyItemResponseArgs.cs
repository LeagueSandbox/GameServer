using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketArgs
{
    public struct BuyItemResponseArgs
    {
        public uint UnitNetId { get; }
        public int ItemId { get; }
        public byte ItemSlot { get; }
        public byte StackSize { get; }
        public ItemOwnerType ItemOwner { get; }

        public BuyItemResponseArgs(uint unitNetId, int itemId, byte itemSlot, byte stackSize, ItemOwnerType itemOwner)
        {
            UnitNetId = unitNetId;
            ItemId = itemId;
            ItemSlot = itemSlot;
            StackSize = stackSize;
            ItemOwner = itemOwner;
        }
    }
}
