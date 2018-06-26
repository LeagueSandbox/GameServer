using ENet;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleStatsConfirm : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2_S_STATS_CONFIRM;
        public override Channel PacketChannel => Channel.CHL_C2_S;

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            return true;
        }
    }
}
