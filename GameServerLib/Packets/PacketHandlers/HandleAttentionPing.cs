using GameServerCore;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using LeaguePackets.Game;
using System.Numerics;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleAttentionPing : PacketHandlerBase<C2S_MapPing>
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public HandleAttentionPing(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, C2S_MapPing req)
        {
            var client = _playerManager.GetPeerInfo(userId);
            _game.PacketNotifier.NotifyS2C_MapPing(req.Position, (Pings)req.PingCategory, req.TargetNetID, client);
            return true;
        }
    }
}
