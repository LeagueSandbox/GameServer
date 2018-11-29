using GameServerCore;
using GameServerCore.Enums;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleExit : PacketHandlerBase<ExitRequest>
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public HandleExit(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, ExitRequest req)
        {
            var peerinfo = _playerManager.GetPeerInfo(userId);
            _game.PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.SUMMONER_DISCONNECTED, peerinfo.Champion);
            peerinfo.IsDisconnected = true;

            return true;
        }
    }
}