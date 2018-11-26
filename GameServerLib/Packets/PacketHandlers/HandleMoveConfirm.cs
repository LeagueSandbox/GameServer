using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleMoveConfirm : PacketHandlerBase<MoveConfirmRequest>
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_MOVE_CONFIRM;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleMoveConfirm(Game game) { }

        public override bool HandlePacket(int userId, MoveConfirmRequest req)
        {
            // TODO: check movement cheat
            return true;
        }
    }
}
