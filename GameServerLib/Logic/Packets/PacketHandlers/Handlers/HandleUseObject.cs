using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public class HandleUseObject : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly Logger _logger;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_UseObject;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleUseObject(Game game, Logger logger, PlayerManager playerManager)
        {
            _game = game;
            _logger = logger;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var parsedData = new UseObject(data);
            _logger.LogCoreInfo($"Object {_playerManager.GetPeerInfo(peer).Champion.NetId} is trying to use (right clicked) {parsedData.targetNetId}");

            return true;
        }
    }
}
