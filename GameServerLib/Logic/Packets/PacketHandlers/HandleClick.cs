using ENet;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.Logic.Attributes;
using LeagueSandbox.GameServer.Logic.Logging;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    [DisabledHandler]
    public class HandleClick : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly ILogger _logger;
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
            _logger.Info(msg);

            return true;
        }
    }
}
