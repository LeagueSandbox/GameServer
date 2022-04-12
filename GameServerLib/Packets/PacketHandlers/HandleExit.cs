using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore;
using GameServerCore.Packets.Handlers;
using LeaguePackets.Game.Events;

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
            var annoucement = new OnQuit { OtherNetID = peerinfo.Champion.NetId };
            _game.PacketNotifier.NotifyS2C_OnEventWorld(annoucement, peerinfo.Champion.NetId);
            peerinfo.IsDisconnected = true;

            return true;
        }
    }
}