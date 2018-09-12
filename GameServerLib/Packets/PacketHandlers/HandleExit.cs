using GameServerCore;
using GameServerCore.Enums;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleExit : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_EXIT;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleExit(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, byte[] data)
        {
            var peerinfo = _playerManager.GetPeerInfo(userId);
            _game.PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.SUMMONER_DISCONNECTED, peerinfo.Champion);
            peerinfo.IsDisconnected = true;

            return true;
        }
    }
}