using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Packets.Handlers;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleStatsConfirm : PacketHandlerBase<ReplicationConfirmRequest>
    {
        public HandleStatsConfirm(Game game) { }

        public override bool HandlePacket(int userId, ReplicationConfirmRequest req)
        {
            return true;
        }
    }
}
