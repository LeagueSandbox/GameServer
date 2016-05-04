using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleMap : IPacketHandler
    {
        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            //Builds team info
            var screenInfo = new LoadScreenInfo(game.GetPlayers());
            bool pInfo = game.GetPacketHandlerManager().sendPacket(peer, screenInfo, Channel.CHL_LOADING_SCREEN);

            //For all players send this info
            bool bOk = false;
            foreach (var player in game.GetPlayers())
            {
                var loadName = new LoadScreenPlayerName(player);
                var loadChampion = new LoadScreenPlayerChampion(player);
                bool pName = game.GetPacketHandlerManager().sendPacket(peer, loadName, Channel.CHL_LOADING_SCREEN);
                bool pHero = game.GetPacketHandlerManager().sendPacket(peer, loadChampion, Channel.CHL_LOADING_SCREEN);

                bOk = (pName && pHero);

                if (!bOk)
                    break;
            }

            return (pInfo && bOk);
        }
    }
}
