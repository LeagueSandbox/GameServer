using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Packets.Handlers;
using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleLoadPing : PacketHandlerBase<PingLoadInfoRequest>
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public HandleLoadPing(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, PingLoadInfoRequest req)
        {
            var peerInfo = _playerManager.GetPeerInfo(userId);
            if (peerInfo == null)
            {
                return false;
            }

            _game.PacketNotifier.NotifyPingLoadInfo(peerInfo, req);
            return true;
        }
    }
}
