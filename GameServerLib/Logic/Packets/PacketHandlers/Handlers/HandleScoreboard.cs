using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public class HandleScoreboard : PacketHandlerBase
    {
        private readonly PlayerManager _playerManager;
        private readonly Logger _logger;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_Scoreboard;
        public override Channel PacketChannel => Channel.CHL_C2S;

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
