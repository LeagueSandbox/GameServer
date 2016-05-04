using ENet;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleKeyCheck : IPacketHandler
    {
        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var keyCheck = new KeyCheck(data);
            var userId = game.getBlowfish().Decrypt(keyCheck.checkId);

            if (userId != keyCheck.userId)
                return false;

            var playerNo = 0;

            foreach (var p in game.getPlayers())
            {
                var player = p.Item2;
                if (player.userId == userId)
                {
                    if (player.getPeer() != null)
                    {
                        Logger.LogCoreWarning("Ignoring new player " + userId + ", already connected!");
                        return false;
                    }

                    //TODO: add at least port or smth
                    p.Item1 = peer.Address.port;
                    player.setPeer(peer);
                    var response = new KeyCheck(keyCheck.userId, playerNo);
                    bool bRet = PacketHandlerManager.getInstace().sendPacket(peer, response, Channel.CHL_HANDSHAKE);
                    handleGameNumber(player, peer);//Send 0x91 Packet?
                    return true;
                }
                ++playerNo;
            }
            return false;
        }

        bool handleGameNumber(ClientInfo client, Peer peer)
        {
            var world = new WorldSendGameNumber(1, client.getName());
            return PacketHandlerManager.getInstace().sendPacket(peer, world, Channel.CHL_S2C);
        }
    }
}
