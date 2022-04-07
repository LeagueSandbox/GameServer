using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;
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
