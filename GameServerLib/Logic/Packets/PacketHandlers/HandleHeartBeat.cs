using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleHeartBeat : PacketHandlerBase
    {
        private readonly Logger _logger;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_HEART_BEAT;
        public override Channel PacketChannel => Channel.CHL_GAMEPLAY;

        public HandleHeartBeat(Logger logger, PlayerManager playerManager)
        {
            _logger = logger;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var heartbeat = new HeartBeat(data);

            var diff = heartbeat.AckTime - heartbeat.ReceiveTime;
            if (heartbeat.ReceiveTime > heartbeat.AckTime)
            {
                var peerInfo = _playerManager.GetPeerInfo(peer);
                var msg = $"Player {peerInfo.UserId} sent an invalid heartbeat - Timestamp error (diff: {diff})";
                _logger.LogCoreWarning(msg);
            }

            return true;
        }
    }
}
