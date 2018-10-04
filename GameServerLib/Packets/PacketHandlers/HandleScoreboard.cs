using GameServerCore;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleScoreboard : PacketHandlerBase
    {
        private readonly IPlayerManager _playerManager;
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

        public override bool HandlePacket(int userId, byte[] data)
        {
            _logger.Debug($"Player {_playerManager.GetPeerInfo(userId).Name} has looked at the scoreboard.");
            // Send to that player stats packet
            var champion = _playerManager.GetPeerInfo(userId).Champion;
             _game.PacketNotifier.NotifyPlayerStats(champion);
            return true;
        }
    }
}
