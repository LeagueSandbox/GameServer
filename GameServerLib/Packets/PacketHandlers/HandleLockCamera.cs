using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleLockCamera : PacketHandlerBase<LockCameraRequest>
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_LOCK_CAMERA;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleLockCamera(Game game) { }

        public override bool HandlePacket(int userId, LockCameraRequest req)
        {
            return true;
        }
    }
}
