using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Items;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleSellItem : IPacketHandler
    {
        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var sell = new SellItem(data);
            var client = game.GetPeerInfo(peer);

            var i = game.GetPeerInfo(peer).GetChampion().getInventory().GetItem(sell.slotId);
            if (i == null)
                return false;

            float sellPrice = i.ItemType.TotalPrice * i.ItemType.TotalPrice;
            client.GetChampion().getStats().setGold(client.GetChampion().getStats().getGold() + sellPrice);

            if (i.ItemType.MaxStack > 1)
            {
                i.DecrementStackSize();
                game.GetPacketNotifier().notifyRemoveItem(client.GetChampion(), sell.slotId, i.StackSize);

                if (i.StackSize == 0)
                    client.GetChampion().getInventory().RemoveItem(sell.slotId);
            }
            else
            {
                game.GetPacketNotifier().notifyRemoveItem(client.GetChampion(), sell.slotId, 0);
                client.GetChampion().getInventory().RemoveItem(sell.slotId);
            }

            client.GetChampion().getStats().unapplyStatMods(i.ItemType.StatMods);

            return true;
        }
    }
}
