using ENet;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleScoreboard : PacketHandlerBase
    {

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_SCOREBOARD;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            Logger.LogCoreInfo($"Player {PlayerManager.GetPeerInfo(peer).Name} has looked at the scoreboard.");
            return true;
        }
    }
}
