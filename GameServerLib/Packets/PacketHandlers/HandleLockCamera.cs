using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleLockCamera : PacketHandlerBase<LockCameraRequest>
    {
        public HandleLockCamera(Game game) { }

        public override bool HandlePacket(int userId, LockCameraRequest req)
        {
            return true;
        }
    }
}
