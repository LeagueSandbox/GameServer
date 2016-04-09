using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static ENet.Native;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleKeyCheck : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
        {
            // players[0].Item1 = peer->address.host; //temp
            //   players[0].Item2.setPeer(enetEvent.peer);

            var keyCheck = new KeyCheck(data);
            var userId = (long)BlowFishCS.BlowFishCS.Decrypt2(game.getBlowfish(), (ulong)keyCheck.checkId);

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
                    p.Item1 = peer->address.host;
                    player.setPeer(peer);
                    var response = new KeyCheck(keyCheck.userId, playerNo);
                    bool bRet = PacketHandlerManager.getInstace().sendPacket(peer, response, Channel.CHL_HANDSHAKE);
                    handleGameNumber(player, peer, null);//Send 0x91 Packet?
                    return true;
                }
                ++playerNo;
            }
            return false;
        }

        unsafe bool handleGameNumber(ClientInfo client, ENetPeer* peer, ENetPacket* packet)
        {
            var world = new WorldSendGameNumber(1, client.getName());
            return PacketHandlerManager.getInstace().sendPacket(peer, world, Channel.CHL_S2C);
        }
    }
}
