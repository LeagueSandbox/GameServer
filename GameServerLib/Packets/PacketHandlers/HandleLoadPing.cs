using GameServerCore;
using GameServerCore.Packets.Handlers;
using LeaguePackets.Game;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleLoadPing : PacketHandlerBase<C2S_Ping_Load_Info>
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public HandleLoadPing(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, C2S_Ping_Load_Info req)
        {
            var peerInfo = _playerManager.GetPeerInfo(userId);
            if (peerInfo == null)
            {
                return false;
            }

             _game.PacketNotifier.NotifyPingLoadInfo(req, peerInfo);
            return true;
        }
    }
}
