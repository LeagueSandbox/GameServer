using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Logging;
using log4net;
using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSync : PacketHandlerBase<SynchVersionRequest>
    {
        private static ILog _logger = LoggerProvider.GetLogger();
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public HandleSync(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, SynchVersionRequest req)
        {
            //Logging->writeLine("Client version: %s", version->version);

            var mapId = _game.Config.GameConfig.Map;
            _logger.Debug("Current map: " + mapId);

            var info = _playerManager.GetPeerInfo(userId);
            var versionMatch = req.Version == Config.VERSION_STRING;
            info.IsMatchingVersion = versionMatch;

            // Version might be an invalid value, currently it trusts the client
            if (!versionMatch)
            {
                _logger.Warn($"Client's version ({req.Version}) does not match server's {Config.VERSION}");
            }
            else
            {
                _logger.Debug("Accepted client version (" + req.Version + ") from client = " + req.ClientID + " & PlayerID = " + info.PlayerId);
            }

            _game.PacketNotifier.NotifySynchVersion(
                userId, _playerManager.GetPlayers(), Config.VERSION_STRING, _game.Config.GameConfig.GameMode, mapId
            );

            return true;
        }
    }
}
