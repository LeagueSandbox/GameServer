using ENet;
using GameServerCore.Packets.Enums;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleStatsConfirm : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_STATS_CONFIRM;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleStatsConfirm(Game game) { }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            return true;
        }
    }
}
