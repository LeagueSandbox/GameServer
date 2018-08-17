using ENet;
using LeagueSandbox.GameServer.Logic.Logging;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleUseObject : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly ILogger _logger;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_USE_OBJECT;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleUseObject(Game game)
        {
            _game = game;
            _logger = LoggerProvider.GetLogger();
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var parsedData = new UseObject(data);
            var champion = _playerManager.GetPeerInfo(peer).Champion;
            var msg = $"Object {champion.NetId} is trying to use (right clicked) {parsedData.TargetNetId}";
            _logger.Info(msg);

            return true;
        }
    }
}
