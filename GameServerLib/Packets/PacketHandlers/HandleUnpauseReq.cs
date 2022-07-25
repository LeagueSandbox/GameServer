using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Packets.Handlers;
using System.Timers;
using LeagueSandbox.GameServer.Players;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleUnpauseReq : PacketHandlerBase<UnpauseRequest>
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

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

            Champion unpauser = null;

            unpauser = _playerManager.GetPeerInfo(userId).Champion;
            foreach (var player in _playerManager.GetPlayers(false))
            {
                _game.PacketNotifier.NotifyResumePacket(unpauser, player, true);
            }
            var timer = new Timer
            {
                AutoReset = false,
                Enabled = true,
                Interval = 5000
            };
            timer.Elapsed += (sender, args) =>
            {
                foreach (var player in _playerManager.GetPlayers(false))
                {
                    _game.PacketNotifier.NotifyResumePacket(unpauser, player, false);
                }
                _game.Unpause();
            };
            return true;
        }
    }
}