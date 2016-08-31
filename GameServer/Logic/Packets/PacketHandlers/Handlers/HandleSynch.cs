using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic;
using Ninject;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleSynch : IPacketHandler
    {
        private Logger _logger = Program.Kernel.Get<Logger>();

        public bool HandlePacket(Peer peer, byte[] data, Game game)
        {
            var version = new SynchVersion(data);
            //Logging->writeLine("Client version: %s", version->version);

            var mapId = game.Config.GameConfig.Map;
            _logger.LogCoreInfo("Current map: " + mapId);

            bool versionMatch = true;
            // Version might be an invalid value, currently it trusts the client
            if (version.version != Config.VERSION)
            {
                versionMatch = false;
                _logger.LogCoreWarning("Client " + version.version + " does not match Server " + Config.VERSION);
            }
            else
            {
                _logger.LogCoreInfo("Accepted client version (" + version.version + ")");
            }

            foreach (var player in game.GetPlayers())
            {
                if (player.Item1 == peer.Address.port)
                {
                    player.Item2.SetVersionMatch(versionMatch);
                    break;
                }
            }
            var answer = new SynchVersionAns(game.GetPlayers(), Config.VERSION, "CLASSIC", mapId);

            return game.PacketHandlerManager.sendPacket(peer, answer, Channel.CHL_S2C);
        }
    }
}
