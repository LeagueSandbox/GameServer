using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public class HandleHeartBeat : PacketHandlerBase
    {
        private readonly Logger _logger;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_HeartBeat;
        public override Channel PacketChannel => Channel.CHL_GAMEPLAY;

        public HandleHeartBeat(Logger logger, PlayerManager playerManager)
        {
            _logger = logger;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var heartbeat = new HeartBeat(data);

            var diff = heartbeat.ackTime - heartbeat.receiveTime;
            if (heartbeat.receiveTime > heartbeat.ackTime)
            {
                _logger.LogCoreWarning($"Player {_playerManager.GetPeerInfo(peer).UserId} sent an invalid heartbeat - Timestamp error (diff: {diff})");
            }
            else
            {
                //  Logger.LogCoreInfo("Player %d sent heartbeat (diff: %.f)", peerInfo(peer)->userId, diff);
            }

            return true;
        }
    }
}
