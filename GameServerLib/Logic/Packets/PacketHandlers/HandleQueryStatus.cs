using ENet;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleQueryStatus : PacketHandlerBase
    {
        private readonly IPacketNotifier _packetNotifier;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_QUERY_STATUS_REQ;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleQueryStatus(Game game)
        {
            _packetNotifier = game.PacketNotifier;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            _packetNotifier.NotifyQueryStatus(peer);
            return true;
        }
    }
}
