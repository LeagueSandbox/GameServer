using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleStatsConfirm : PacketHandlerBase<StatsConfirmRequest>
    {
        public HandleStatsConfirm(Game game) { }

        public override bool HandlePacket(int userId, StatsConfirmRequest req)
        {
            return true;
        }
    }
}
