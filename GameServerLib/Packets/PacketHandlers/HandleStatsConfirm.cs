using GameServerCore.Packets.Handlers;
using LeaguePackets.Game;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleStatsConfirm : PacketHandlerBase<OnReplication_Acc>
    {
        public HandleStatsConfirm(Game game) { }

        public override bool HandlePacket(int userId, OnReplication_Acc req)
        {
            return true;
        }
    }
}
