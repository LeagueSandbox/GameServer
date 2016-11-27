using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleScoreboard : IPacketHandler
    {
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();
        private Logger _logger = Program.ResolveDependency<Logger>();
        public bool HandlePacket(Peer peer, byte[] data)
        {
            _logger.LogCoreInfo("Player " + _playerManager.GetPeerInfo(peer).Name + " has looked at the scoreboard.");
            return true;
        }
    }
}
