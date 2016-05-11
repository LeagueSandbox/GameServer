using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Logic.Content;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleBuyItem : IPacketHandler
    {
        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var request = new BuyItemReq(data);

            var itemTemplate = game.ItemManager.SafeGetItemType(request.id);
            if (itemTemplate == null)
                return false;

            var recipeParts = game.GetPeerInfo(peer).GetChampion().getInventory().GetAvailableItems(itemTemplate.Recipe);
            var price = itemTemplate.TotalPrice;
            Item i;

            if (recipeParts.Count == 0)
            {
                if (game.GetPeerInfo(peer).GetChampion().GetStats().Gold < price)
                {
                    return true;
                }

                i = game.GetPeerInfo(peer).GetChampion().getInventory().AddItem(itemTemplate);

                if (i == null)
                { // Slots full
                    return false;
                }
            }
            else
            {
                foreach (var instance in recipeParts)
                    price -= instance.TotalPrice;

                if (game.GetPeerInfo(peer).GetChampion().GetStats().Gold < price)
                    return false;


                foreach (var instance in recipeParts)
                {
                    game.GetPeerInfo(peer).GetChampion().GetStats().RemoveBuff(instance.ItemType);
                    var champion = game.GetPeerInfo(peer).GetChampion();
                    var inventory = champion.Inventory;
                    game.PacketNotifier.notifyRemoveItem(champion, inventory.GetItemSlot(instance), 0);
                    inventory.RemoveItem(instance);
                }

                i = game.GetPeerInfo(peer).GetChampion().getInventory().AddItem(itemTemplate);
            }

            game.GetPeerInfo(peer).GetChampion().GetStats().Gold = game.GetPeerInfo(peer).GetChampion().GetStats().Gold - price;
            game.GetPeerInfo(peer).GetChampion().GetStats().AddBuff(itemTemplate);
            game.PacketNotifier.notifyItemBought(game.GetPeerInfo(peer).GetChampion(), i);

            return true;
        }
    }
}
