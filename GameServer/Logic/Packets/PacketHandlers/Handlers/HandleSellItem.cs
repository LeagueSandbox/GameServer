﻿using System;
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
            var client = game.getPeerInfo(peer);

            var i = game.getPeerInfo(peer).getChampion().getInventory().GetItem(sell.slotId);
            if (i == null)
                return false;

            float sellPrice = i.ItemType.TotalPrice * i.ItemType.TotalPrice;
            client.getChampion().getStats().Gold = client.getChampion().getStats().Gold + sellPrice;

            if (i.ItemType.MaxStack > 1)
            {
                i.DecrementStackSize();
                PacketNotifier.notifyRemoveItem(client.getChampion(), sell.slotId, i.StackSize);

                if (i.StackSize == 0)
                    client.getChampion().getInventory().RemoveItem(sell.slotId);
            }
            else
            {
                PacketNotifier.notifyRemoveItem(client.getChampion(), sell.slotId, 0);
                client.getChampion().getInventory().RemoveItem(sell.slotId);
            }
            
            client.getChampion().getStats().RemoveBuff(i.ItemType);

            return true;
        }
    }
}
