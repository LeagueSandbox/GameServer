using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using static ENet.Native;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleSwapItems : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
        {
            var request = new SwapItems(data);

            if (request.slotFrom > 6 || request.slotTo > 6)
                return false;

            // "Holy shit this needs refactoring" - Mythic, April 13th 2016
            game.getPeerInfo(peer).getChampion().getInventory().SwapItems(request.slotFrom, request.slotTo);
            PacketNotifier.notifyItemsSwapped(game.getPeerInfo(peer).getChampion(), request.slotFrom, request.slotTo);

            return true;
        }
    }
}
