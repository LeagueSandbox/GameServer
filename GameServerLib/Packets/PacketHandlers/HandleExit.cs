using GameServerCore;
using GameServerCore.Packets.Handlers;
using LeaguePackets.Game;
using LeaguePackets.Game.Events;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleExit : PacketHandlerBase<C2S_Exit>
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public HandleExit(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, C2S_Exit req)
        {
            var peerinfo = _playerManager.GetPeerInfo(userId);
            var annoucement = new OnQuit { OtherNetID = peerinfo.Champion.NetId };
            _game.PacketNotifier.NotifyS2C_OnEventWorld(annoucement, peerinfo.Champion.NetId);
            peerinfo.IsDisconnected = true;

            return true;
        }
    }
}