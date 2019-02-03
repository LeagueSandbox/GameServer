using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleMoveConfirm : PacketHandlerBase<MoveConfirmRequest>
    {

        public HandleMoveConfirm(Game game) { }

        public override bool HandlePacket(int userId, MoveConfirmRequest req)
        {
            // TODO: check movement cheat
            return true;
        }
    }
}
