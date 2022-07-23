using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Packets.Handlers;
using PacketDefinitions420;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleExit : PacketHandlerBase<ExitRequest>
    {
        private readonly PacketHandlerManager _packetHandlerManager;

        public HandleExit(PacketHandlerManager packetHandlerManager)
        {
            _packetHandlerManager = packetHandlerManager;
        }

        public override bool HandlePacket(int userId, ExitRequest req)
        {
            return _packetHandlerManager.HandleDisconnect(userId);
        }
    }
}