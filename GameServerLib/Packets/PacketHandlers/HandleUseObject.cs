using GameServerCore;
using GameServerCore.Packets.Handlers;
using LeaguePackets.Game;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleUseObject : PacketHandlerBase<UseObjectC2S>
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

        public override bool HandlePacket(int userId, UseObjectC2S req)
        {
            var champion = _playerManager.GetPeerInfo(userId).Champion;
            var msg = $"Object {champion.NetId} is trying to use (right clicked) {req.TargetNetID}";
            _logger.Debug(msg);

            return true;
        }
    }
}
