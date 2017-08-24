using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
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
                var peerInfo = _playerManager.GetPeerInfo(peer);
                var msg = $"Player {peerInfo.UserId} sent an invalid heartbeat - Timestamp error (diff: {diff})";
                _logger.LogCoreWarning(msg);
            }

            return true;
        }
    }
}
