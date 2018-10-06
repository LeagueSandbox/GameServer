using GameServerCore;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleUseObject : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly ILog _logger;
        private readonly IPlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_USE_OBJECT;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleUseObject(Game game)
        {
            _game = game;
            _logger = LoggerProvider.GetLogger();
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, byte[] data)
        {
            var request = _game.PacketReader.ReadUseObjectRequest(data);
            var champion = _playerManager.GetPeerInfo(userId).Champion;
            var msg = $"Object {champion.NetId} is trying to use (right clicked) {request.TargetNetId}";
            _logger.Debug(msg);

            return true;
        }
    }
}
