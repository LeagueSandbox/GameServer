using ENet;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets;
using Ninject;
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
        private Logger _logger = Program.Kernel.Get<Logger>();

        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var keyCheck = new KeyCheck(data);
            var userId = game.GetBlowfish().Decrypt(keyCheck.checkId);

            if (userId != keyCheck.userId)
                return false;

            var playerNo = 0;

            foreach (var p in game.GetPlayers())
            {
                var player = p.Item2;
                if (player.UserId == userId)
                {
                    if (player.GetPeer() != null)
                    {
                        _logger.LogCoreWarning("Ignoring new player " + userId + ", already connected!");
                        return false;
                    }

                    //TODO: add at least port or smth
                    p.Item1 = peer.Address.port;
                    player.SetPeer(peer);
                    var response = new KeyCheck(keyCheck.userId, playerNo);
                    bool bRet = game.PacketHandlerManager.sendPacket(peer, response, Channel.CHL_HANDSHAKE);
                    handleGameNumber(player, peer, game);//Send 0x91 Packet?
                    return true;
                }
                ++playerNo;
            }
            return false;
        }

        bool handleGameNumber(ClientInfo client, Peer peer, Game game)
        {
            var world = new WorldSendGameNumber(1, client.GetName());
            return game.PacketHandlerManager.sendPacket(peer, world, Channel.CHL_S2C);
        }
    }
}
