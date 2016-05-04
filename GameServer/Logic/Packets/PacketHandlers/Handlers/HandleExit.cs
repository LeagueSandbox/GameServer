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
    class HandleExit : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
        {
            var peerinfo = game.getPeerInfo(peer);
            Logger.LogCoreInfo("Player " + peerinfo.userId + " exited the game.");
            PacketNotifier.notifyDebugMessage("Player " + peerinfo.userId + " exited the game.");
            peerinfo.Disconnected = true;

            return true;
        }
    }
}
