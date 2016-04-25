using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using static ENet.Native;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Items;
using LeagueSandbox.GameServer.Logic.Content;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleBuyItem : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
        {
            var request = new BuyItemReq(data);

            var itemTemplate = game.ItemManager.SafeGetItemType(request.id);
            if (itemTemplate == null)
                return false;

            var recipeParts = game.getPeerInfo(peer).getChampion().getInventory().GetAvailableItems(itemTemplate.Recipe);
            var price = itemTemplate.TotalPrice;
            Item i;

            if (recipeParts.Count == 0)
            {
                if (game.getPeerInfo(peer).getChampion().getStats().getGold() < price)
                {
                    return true;
                }

                i = game.getPeerInfo(peer).getChampion().getInventory().AddItem(itemTemplate);

                if (i == null)
                { // Slots full
                    return false;
                }
            }
            else
            {
                foreach (var instance in recipeParts)
                    price -= instance.TotalPrice;

                if (game.getPeerInfo(peer).getChampion().getStats().getGold() < price)
                    return false;


                foreach (var instance in recipeParts)
                {
                    game.getPeerInfo(peer).getChampion().getStats().unapplyStatMods(instance.ItemType.StatMods);
                    var champion = game.getPeerInfo(peer).getChampion();
                    var inventory = champion.Inventory;
                    PacketNotifier.notifyRemoveItem(champion, inventory.GetItemSlot(instance), 0);
                    inventory.RemoveItem(instance);
                }

                i = game.getPeerInfo(peer).getChampion().getInventory().AddItem(itemTemplate);
            }

            game.getPeerInfo(peer).getChampion().getStats().setGold(game.getPeerInfo(peer).getChampion().getStats().getGold() - price);
            game.getPeerInfo(peer).getChampion().getStats().applyStatMods(itemTemplate.StatMods);
            PacketNotifier.notifyItemBought(game.getPeerInfo(peer).getChampion(), i);

            return true;
        }
    }
}
