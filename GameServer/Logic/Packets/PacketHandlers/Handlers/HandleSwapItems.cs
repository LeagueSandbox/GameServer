using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleSwapItems : IPacketHandler
    {
        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var request = new SwapItems(data);

            if (request.slotFrom > 6 || request.slotTo > 6)
                return false;

            // "Holy shit this needs refactoring" - Mythic, April 13th 2016
            game.GetPeerInfo(peer).GetChampion().getInventory().SwapItems(request.slotFrom, request.slotTo);
            game.GetPacketNotifier().notifyItemsSwapped(game.GetPeerInfo(peer).GetChampion(), request.slotFrom, request.slotTo);

            return true;
        }
    }
}
