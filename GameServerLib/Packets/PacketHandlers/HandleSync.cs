using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore;
using GameServerCore.Packets.Handlers;
using LeaguePackets.Game;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSync : PacketHandlerBase<SynchVersionRequest>
    {
        private readonly ILog _logger;
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public HandleSync(Game game)
        {
            _logger = LoggerProvider.GetLogger();
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, SynchVersionRequest req)
        {
            //Logging->writeLine("Client version: %s", version->version);

            var mapId = _game.Config.GameConfig.Map;
            _logger.Debug("Current map: " + mapId);

            var versionMatch = true;
            // Version might be an invalid value, currently it trusts the client
            if (req.Version != Config.VERSION_STRING)
            {
                versionMatch = false;
                _logger.Warn($"Client's version ({req.Version}) does not match server's {Config.VERSION}");
            }
            else
            {
                _logger.Debug("Accepted client version (" + req.Version + ") from client = " + req.ClientID + " & PlayerID = " + userId);
            }

            foreach (var player in _playerManager.GetPlayers())
            {
                if (player.Item1 == userId)
                {
                    player.Item2.IsMatchingVersion = versionMatch;
                    break;
                }
            }

            _game.PacketNotifier.NotifySynchVersion(userId, _playerManager.GetPlayers(), Config.VERSION_STRING, _game.Config.GameConfig.GameMode,
               mapId);

            return true;
        }
    }
}
