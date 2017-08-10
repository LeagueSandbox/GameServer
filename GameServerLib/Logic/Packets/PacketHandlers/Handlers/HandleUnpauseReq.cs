using System.Timers;
using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public class HandleUnpauseReq : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_UnpauseGame;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleUnpauseReq(Game game, PlayerManager playerManager)
        {
            _game = game;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            if (!_game.IsPaused)
            {
                return false;
            }

            Champion unpauser;
            if (peer == null)
            {
                unpauser = null;
            }
            else
            {
                unpauser = _playerManager.GetPeerInfo(peer).Champion;
            }

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