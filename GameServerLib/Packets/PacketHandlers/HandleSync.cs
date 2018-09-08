using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Logging;
using GameServerCore;
using log4net;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSync : PacketHandlerBase
    {
        private readonly ILog _logger;
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SYNCH_VERSION;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleSync(Game game)
        {
            _logger = LoggerProvider.GetLogger();
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, byte[] data)
        {
            var request = _game.PacketReader.ReadSynchVersionRequest(data);
            //Logging->writeLine("Client version: %s", version->version);

            var mapId = _game.Config.GameConfig.Map;
            _logger.Debug("Current map: " + mapId);

            var versionMatch = true;
            // Version might be an invalid value, currently it trusts the client
            if (request.Version != Config.VERSION_STRING)
            {
                versionMatch = false;
                _logger.Warn($"Client's version ({request.Version}) does not match server's {Config.VERSION}");
            }
            else
            {
                _logger.Debug("Accepted client version (" + request.Version + ")");
            }

            foreach (var player in _playerManager.GetPlayers())
            {
                if (player.Item1 == userId)
                {
                    player.Item2.IsMatchingVersion = versionMatch;
                    break;
                }
            }

             _game.PacketNotifier.NotifySynchVersion(userId, _playerManager.GetPlayers(), Config.VERSION_STRING, "CLASSIC",
                mapId);

            return true;
        }
    }
}
