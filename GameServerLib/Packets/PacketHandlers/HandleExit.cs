using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Packets.Handlers;
using LeaguePackets.Game.Events;
using PacketDefinitions420;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleExit : PacketHandlerBase<ExitRequest>
    {
        private readonly IPacketHandlerManager _packetHandlerManager;

        public HandleExit(IPacketHandlerManager packetHandlerManager)
        {
            _packetHandlerManager = packetHandlerManager;
        }

        public override bool HandlePacket(int userId, ExitRequest req)
        {
            return _packetHandlerManager.HandleDisconnect(userId);
        }
    }
}