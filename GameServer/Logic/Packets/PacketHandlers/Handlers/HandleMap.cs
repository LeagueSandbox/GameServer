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
    class HandleMap : IPacketHandler
    {
        public unsafe bool HandlePacket(ENetPeer* peer, byte[] data, Game game)
        {
            //Builds team info
            var screenInfo = new LoadScreenInfo(game.getPlayers());
            bool pInfo = PacketHandlerManager.getInstace().sendPacket(peer, screenInfo, Channel.CHL_LOADING_SCREEN);

            //For all players send this info
            bool bOk = false;
            foreach (var player in game.getPlayers())
            {
                var loadName = new LoadScreenPlayerName(player);
                var loadChampion = new LoadScreenPlayerChampion(player);
                bool pName = PacketHandlerManager.getInstace().sendPacket(peer, loadName, Channel.CHL_LOADING_SCREEN);
                bool pHero = PacketHandlerManager.getInstace().sendPacket(peer, loadChampion, Channel.CHL_LOADING_SCREEN);

                bOk = (pName && pHero);

                if (!bOk)
                    break;
            }

            return (pInfo && bOk);
        }
    }
}
