using GameServerCore;
using GameServerCore.Packets.Handlers;
using LeaguePackets.Game;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleScoreboard : PacketHandlerBase<C2S_StatsUpdateReq>
    {
        private readonly IPlayerManager _playerManager;
        private readonly ILog _logger;
        private readonly Game _game;

        public HandleScoreboard(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
            _logger = LoggerProvider.GetLogger();
        }

        public override bool HandlePacket(int userId, C2S_StatsUpdateReq req)
        {
            _logger.Debug($"Player {_playerManager.GetPeerInfo(userId).Name} has looked at the scoreboard.");
            // Send to that player stats packet
            var champion = _playerManager.GetPeerInfo(userId).Champion;
             _game.PacketNotifier.NotifyS2C_HeroStats(champion);
            return true;
        }
    }
}
