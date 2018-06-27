using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleSynch : PacketHandlerBase
    {
        private readonly Logger _logger;
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2_S_SYNCH_VERSION;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public HandleSynch(Logger logger, Game game, PlayerManager playerManager)
        {
            _logger = logger;
            _game = game;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var version = new SynchVersionRequest(data);
            //Logging->writeLine("Client version: %s", version->version);

            var mapId = _game.Config.GameConfig.Map;
            _logger.LogCoreInfo("Current map: " + mapId);

            var versionMatch = true;
            // Version might be an invalid value, currently it trusts the client
            if (version.Version != Config.VERSION)
            {
                versionMatch = false;
                _logger.LogCoreWarning("Client " + version.Version + " does not match Server " + Config.VERSION);
            }
            else
            {
                _logger.LogCoreInfo("Accepted client version (" + version.Version + ")");
            }

            foreach (var player in _playerManager.GetPlayers())
            {
                if (player.Item1 == peer.Address.port)
                {
                    player.Item2.IsMatchingVersion = versionMatch;
                    break;
                }
            }
            var answer = new SynchVersionResponse(_playerManager.GetPlayers(), Config.VERSION, "CLASSIC", mapId);

            return _game.PacketHandlerManager.SendPacket(peer, answer, Channel.CHL_S2_C);
        }
    }
}
