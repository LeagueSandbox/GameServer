using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleAttentionPing : PacketHandlerBase<AttentionPingRequest>
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public HandleAttentionPing(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, AttentionPingRequest req)
        {
            var client = _playerManager.GetPeerInfo(userId);
            _game.PacketNotifier.NotifyS2C_MapPing(req.Position, req.PingCategory, req.TargetNetID, client);
            return true;
        }
    }
}
