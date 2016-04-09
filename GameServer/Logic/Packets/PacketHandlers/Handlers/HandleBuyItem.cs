using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using static ENet.Native;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Items;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleBuyItem : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
        {
            var request = new BuyItemReq(data);

            var itemTemplate = ItemManager.getInstance().getItemTemplateById(request.id);
            if (itemTemplate == null)
                return false;

            var recipeParts = game.getPeerInfo(peer).getChampion().getInventory().getAvailableRecipeParts(itemTemplate);
            var price = itemTemplate.getTotalPrice();
            ItemInstance i;

            if (recipeParts.Count == 0)
            {
                if (game.getPeerInfo(peer).getChampion().getStats().getGold() < price)
                {
                    return true;
                }

                i = game.getPeerInfo(peer).getChampion().getInventory().addItem(itemTemplate);

                if (i == null)
                { // Slots full
                    return false;
                }
            }
            else
            {
                foreach (var instance in recipeParts)
                    price -= instance.getTemplate().getTotalPrice();

                if (game.getPeerInfo(peer).getChampion().getStats().getGold() < price)
                    return false;


                foreach (var instance in recipeParts)
                {
                    game.getPeerInfo(peer).getChampion().getStats().unapplyStatMods(instance.getTemplate().getStatMods());
                    PacketNotifier.notifyRemoveItem(game.getPeerInfo(peer).getChampion(), instance.getSlot(), 0);
                    game.getPeerInfo(peer).getChampion().getInventory().removeItem(instance.getSlot());
                }

                i = game.getPeerInfo(peer).getChampion().getInventory().addItem(itemTemplate);
            }

            game.getPeerInfo(peer).getChampion().getStats().setGold(game.getPeerInfo(peer).getChampion().getStats().getGold() - price);
            game.getPeerInfo(peer).getChampion().getStats().applyStatMods(itemTemplate.getStatMods());
            PacketNotifier.notifyItemBought(game.getPeerInfo(peer).getChampion(), i);

            return true;
        }
    }
}
