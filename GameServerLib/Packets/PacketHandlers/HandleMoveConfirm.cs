using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleMoveConfirm : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_MOVE_CONFIRM;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleMoveConfirm(Game game) { }

        public override bool HandlePacket(int userId, byte[] data)
        {
            return true;
        }
    }
}
