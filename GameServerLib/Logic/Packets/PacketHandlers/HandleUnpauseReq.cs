using System.Timers;
using ENet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleUnpauseReq : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_UNPAUSEGame;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            if (!Game.IsPaused)
            {
                return false;
            }

            Champion unpauser = null;
            if (peer != null)
            {
                unpauser = PlayerManager.GetPeerInfo(peer).Champion;
            }

            Game.PacketNotifier.NotifyResumeGame(unpauser, true);
            var timer = new Timer
            {
                AutoReset = false,
                Enabled = true,
                Interval = 5000
            };
            timer.Elapsed += (sender, args) =>
            {
                Game.PacketNotifier.NotifyResumeGame(unpauser, false);
                Game.Unpause();
            };
            return true;
        }
    }
}