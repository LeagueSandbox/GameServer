using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleSellItem : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SELL_ITEM;
        public override Channel PacketChannel => Channel.CHL_C2_S;
        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var sell = new SellItem(data);
            var client = PlayerManager.GetPeerInfo(peer);

            var i = PlayerManager.GetPeerInfo(peer).Champion.GetInventory().GetItem(sell.SlotId);
            if (i == null)
            {
                return false;
            }

            var sellPrice = i.ItemType.TotalPrice * i.ItemType.SellBackModifier;
            client.Champion.Stats.Gold += sellPrice;

            if (i.ItemType.MaxStack > 1)
            {
                i.DecrementStackSize();
                Game.PacketNotifier.NotifyRemoveItem(client.Champion, sell.SlotId, i.StackSize);
                if (i.StackSize == 0)
                {
                    client.Champion.GetInventory().RemoveItem(sell.SlotId);
                }
            }
            else
            {
                Game.PacketNotifier.NotifyRemoveItem(client.Champion, sell.SlotId, 0);
                client.Champion.GetInventory().RemoveItem(sell.SlotId);
            }

            client.Champion.Stats.RemoveModifier(i.ItemType);

            return true;
        }
    }
}
