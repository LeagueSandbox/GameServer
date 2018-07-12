using ENet;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleExit : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_EXIT;
        public override Channel PacketChannel => Channel.CHL_C2_S;
        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var peerinfo = PlayerManager.GetPeerInfo(peer);
            Game.PacketNotifier.NotifyUnitAnnounceEvent(UnitAnnounces.SUMMONER_DISCONNECTED, peerinfo.Champion);
            peerinfo.IsDisconnected = true;

            return true;
        }
    }
}