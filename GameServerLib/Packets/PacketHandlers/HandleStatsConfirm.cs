using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleStatsConfirm : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_STATS_CONFIRM;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleStatsConfirm(Game game) { }

        public override bool HandlePacket(int userId, byte[] data)
        {
            return true;
        }
    }
}
