using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ENet;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic;
using Ninject;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleSynch : IPacketHandler
    {
        private Logger _logger = Program.ResolveDependency<Logger>();
        private Game _game = Program.ResolveDependency<Game>();
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var version = new SynchVersion(data);
            //Logging->writeLine("Client version: %s", version->version);

            var mapId = _game.Config.GameConfig.Map;
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

            foreach (var player in _playerManager.GetPlayers())
            {
                if (player.Item1 == peer.Address.port)
                {
                    player.Item2.IsMatchingVersion = versionMatch;
                    break;
                }
            }
            var answer = new SynchVersionAns(_playerManager.GetPlayers(), Config.VERSION, "CLASSIC", mapId);

            return _game.PacketHandlerManager.sendPacket(peer, answer, Channel.CHL_S2C);
        }
    }
}
