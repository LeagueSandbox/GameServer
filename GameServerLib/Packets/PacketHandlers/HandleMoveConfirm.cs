using GameServerCore.Packets.Handlers;
using LeaguePackets.Game;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleMoveConfirm : PacketHandlerBase<Waypoint_Acc>
    {

        public HandleMoveConfirm(Game game) { }

        public override bool HandlePacket(int userId, Waypoint_Acc req)
        {
            // TODO: check movement cheat
            return true;
        }
    }
}
