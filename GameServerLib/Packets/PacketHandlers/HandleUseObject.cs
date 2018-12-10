using GameServerCore;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleUseObject : PacketHandlerBase<UseObjectRequest>
    {
        private readonly Game _game;
        private readonly ILog _logger;
        private readonly IPlayerManager _playerManager;

        public HandleUseObject(Game game)
        {
            _game = game;
            _logger = LoggerProvider.GetLogger();
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, UseObjectRequest req)
        {
            var champion = _playerManager.GetPeerInfo(userId).Champion;
            var msg = $"Object {champion.NetId} is trying to use (right clicked) {req.TargetNetId}";
            _logger.Debug(msg);

            return true;
        }
    }
}
