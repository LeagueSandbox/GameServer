using ENet;
using GameServerCore.Packets.Enums;
using LeagueSandbox.GameServer.Attributes;
using LeagueSandbox.GameServer.Logging;
using LeagueSandbox.GameServer.Players;
using log4net;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    [DisabledHandler]
    public class HandleClick : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly ILog _logger;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CLICK;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleClick(Game game)
        {
            _game = game;
            _logger = LoggerProvider.GetLogger();
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _game.PacketReader.ReadClickRequest(data);
            var msg = $"Object {_playerManager.GetPeerInfo(peer).Champion.NetId} clicked on {request.TargetNetId}";
            _logger.Debug(msg);

            return true;
        }
    }
}
