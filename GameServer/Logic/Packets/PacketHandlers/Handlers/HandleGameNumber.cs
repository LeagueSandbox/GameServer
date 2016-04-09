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
    class HandleGameNumber : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
        {
            var world = new WorldSendGameNumber(1, game.getPeerInfo(peer).getName());
            return PacketHandlerManager.getInstace().sendPacket(peer, world, Channel.CHL_S2C);
        }
    }
}
