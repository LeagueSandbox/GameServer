using ENet;
using GameServerCore.Packets.Enums;
using LeagueSandbox.GameServer.Logging;
using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSync : PacketHandlerBase
    {
        private readonly ILogger _logger;
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SYNCH_VERSION;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleSync(Game game)
        {
            _logger = LoggerProvider.GetLogger();
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _game.PacketReader.ReadSynchVersionRequest(data);
            //Logging->writeLine("Client version: %s", version->version);

            var mapId = _game.Config.GameConfig.Map;
            _logger.Info("Current map: " + mapId);

            var versionMatch = true;
            // Version might be an invalid value, currently it trusts the client
            if (request.Version != Config.VERSION_STRING)
            {
                versionMatch = false;
                _logger.Warning($"Client's version ({request.Version}) does not match server's {Config.VERSION}");
            }
            else
            {
                _logger.Info("Accepted client version (" + request.Version + ")");
            }

            foreach (var player in _playerManager.GetPlayers())
            {
                if (player.Item1 == peer.Address.port)
                {
                    player.Item2.IsMatchingVersion = versionMatch;
                    break;
                }
            }

             _game.PacketNotifier.NotifySynchVersion(peer, _playerManager.GetPlayers(), Config.VERSION_STRING, "CLASSIC",
                mapId);

            return true;
        }
    }
}
