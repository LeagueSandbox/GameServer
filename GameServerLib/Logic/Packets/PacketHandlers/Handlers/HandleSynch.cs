using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public class HandleSynch : PacketHandlerBase
    {
        private readonly Logger _logger;
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SynchVersion;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleSynch(Logger logger, Game game, PlayerManager playerManager)
        {
            _logger = logger;
            _game = game;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
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
