using ENet;
using LeagueSandbox.GameServer.Logic.Logging;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleHeartBeat : PacketHandlerBase
    {
        private readonly IPacketReader _packetReader;
        private readonly ILogger _logger;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_HEART_BEAT;
        public override Channel PacketChannel => Channel.CHL_GAMEPLAY;

        public HandleHeartBeat(Game game)
        {
            _packetReader = game.PacketReader;
            _logger = LoggerProvider.GetLogger();
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _packetReader.ReadHeartbeatRequest(data);
            var diff = request.AckTime - request.ReceiveTime;
            if (request.ReceiveTime > request.AckTime)
            {
                var peerInfo = _playerManager.GetPeerInfo(peer);
                var msg = $"Player {peerInfo.UserId} sent an invalid heartbeat - Timestamp error (diff: {diff})";
                _logger.Warning(msg);
            }

            return true;
        }
    }
}
