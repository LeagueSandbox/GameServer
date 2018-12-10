using GameServerCore;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;
using System.Numerics;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleAttentionPing : PacketHandlerBase<AttentionPingRequest>
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public HandleAttentionPing(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, AttentionPingRequest req)
        {
            var client = _playerManager.GetPeerInfo(userId);
            _game.PacketNotifier.NotifyPing(client, new Vector2(req.X, req.Y), req.TargetNetId, req.Type);
            return true;
        }
    }
}
