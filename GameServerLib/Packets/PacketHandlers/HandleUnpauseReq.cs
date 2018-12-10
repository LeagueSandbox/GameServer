using GameServerCore;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;
using System.Timers;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleUnpauseReq : PacketHandlerBase<UnpauseRequest>
    {
        private readonly Game _game;
        private readonly IPlayerManager _playerManager;

        public HandleUnpauseReq(Game game)
        {
            _game = game;
            _playerManager = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, UnpauseRequest req)
        {
            if (!_game.IsPaused)
            {
                return false;
            }

            IChampion unpauser = null;

            unpauser = _playerManager.GetPeerInfo(userId).Champion;

            _game.PacketNotifier.NotifyResumeGame(unpauser, true);
            var timer = new Timer
            {
                AutoReset = false,
                Enabled = true,
                Interval = 5000
            };
            timer.Elapsed += (sender, args) =>
            {
                _game.PacketNotifier.NotifyResumeGame(unpauser, false);
                _game.Unpause();
            };
            return true;
        }
    }
}