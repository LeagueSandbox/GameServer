using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleMap : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            //Builds team info
            var screenInfo = new LoadScreenInfo(_playerManager.GetPlayers());
            bool pInfo = _game.PacketHandlerManager.sendPacket(peer, screenInfo, Channel.CHL_LOADING_SCREEN);

            //For all players send this info
            bool bOk = false;
            foreach (var player in _playerManager.GetPlayers())
            {
                var loadName = new LoadScreenPlayerName(player);
                var loadChampion = new LoadScreenPlayerChampion(player);
                bool pName = _game.PacketHandlerManager.sendPacket(peer, loadName, Channel.CHL_LOADING_SCREEN);
                bool pHero = _game.PacketHandlerManager.sendPacket(peer, loadChampion, Channel.CHL_LOADING_SCREEN);

                bOk = (pName && pHero);

                if (!bOk)
                    break;
            }

            return (pInfo && bOk);
        }
    }
}
