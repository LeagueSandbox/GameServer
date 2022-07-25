using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Logging;
using log4net;
using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSyncSimTime : PacketHandlerBase<SyncSimTimeRequest>
    {
        private readonly Game _game;
        private static ILog _logger = LoggerProvider.GetLogger();
        private readonly PlayerManager _playerManager;

        public HandleSyncSimTime(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, SyncSimTimeRequest req)
        {
            //Check this
            var diff = req.TimeLastServer - req.TimeLastClient;
            if (req.TimeLastClient > req.TimeLastServer)
            {
                var peerInfo = _playerManager.GetPeerInfo(userId);
                var msg = $"Client {peerInfo.ClientId} sent an invalid heartbeat - Timestamp error (diff: {diff})";
                _logger.Warn(msg);
            }

            return true;
        }
    }
}
