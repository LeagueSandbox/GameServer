using GameServerCore.Packets.Handlers;
using LeaguePackets.Game;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleLockCamera : PacketHandlerBase<World_LockCamera_Server>
    {
        public HandleLockCamera(Game game) { }

        public override bool HandlePacket(int userId, World_LockCamera_Server req)
        {
            return true;
        }
    }
}
