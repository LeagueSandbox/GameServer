using ENet;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.Logic.Logging;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleScoreboard : PacketHandlerBase
    {
        private readonly IPacketNotifier _packetNotifier;
        private readonly PlayerManager _playerManager;
        private readonly ILogger _logger;
        private readonly Game _game;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SCOREBOARD;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleScoreboard(Game game)
        {
            _packetNotifier = game.PacketNotifier;
            _game = game;
            _playerManager = game.PlayerManager;
            _logger = LoggerProvider.GetLogger();
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            _logger.Info($"Player {_playerManager.GetPeerInfo(peer).Name} has looked at the scoreboard.");
            // Send to that player stats packet
            var champion = _playerManager.GetPeerInfo(peer).Champion;
            _packetNotifier.NotifyPlayerStats(champion);
            return true;
        }
    }
}
