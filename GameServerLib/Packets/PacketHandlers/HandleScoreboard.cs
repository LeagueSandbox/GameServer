using ENet;
using GameServerCore.Packets.Enums;
using LeagueSandbox.GameServer.Logging;
using LeagueSandbox.GameServer.Players;
using log4net;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleScoreboard : PacketHandlerBase
    {
        private readonly PlayerManager _playerManager;
        private readonly ILog _logger;
        private readonly Game _game;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SCOREBOARD;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleScoreboard(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
            _logger = LoggerProvider.GetLogger();
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            _logger.Debug($"Player {_playerManager.GetPeerInfo(peer).Name} has looked at the scoreboard.");
            // Send to that player stats packet
            var champion = _playerManager.GetPeerInfo(peer).Champion;
             _game.PacketNotifier.NotifyPlayerStats(champion);
            return true;
        }
    }
}
