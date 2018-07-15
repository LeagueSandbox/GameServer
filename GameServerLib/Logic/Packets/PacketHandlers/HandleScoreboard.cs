using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleScoreboard : PacketHandlerBase
    {
        private readonly PlayerManager _playerManager;
        private readonly Logger _logger;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SCOREBOARD;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public HandleScoreboard(PlayerManager playerManager, Logger logger)
        {
            _playerManager = playerManager;
            _logger = logger;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            _logger.LogCoreInfo($"Player {_playerManager.GetPeerInfo(peer).Name} has looked at the scoreboard.");
            return true;
        }
    }
}
