using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using static ENet.Native;
using IntWarsSharp.Logic.Packets;

namespace IntWarsSharp.Core.Logic.PacketHandlers.Packets
{
    class HandleSwapItems : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
        {
            var request = new SwapItems(data);

            if (request.slotFrom > 6 || request.slotTo > 6)
                return false;

            game.getPeerInfo(peer).getChampion().getInventory().swapItems(request.slotFrom, request.slotTo);
            PacketNotifier.notifyItemsSwapped(game.getPeerInfo(peer).getChampion(), request.slotFrom, request.slotTo);

            return true;
        }
    }
}
