using ENet;
using LeagueSandbox.GameServer.Logic.Players;
using System.Timers;
using LeagueSandbox.GameServer.Logic.GameObjects;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleUnpauseReq : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        public bool HandlePacket(Peer peer, byte[] data)
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