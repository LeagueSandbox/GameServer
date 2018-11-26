using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleQueryStatus : PacketHandlerBase<QueryStatusRequest>
    {
        private readonly Game _game;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_QUERY_STATUS_REQ;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleQueryStatus(Game game)
        {
            _game = game;
        }

        public override bool HandlePacket(int userId, QueryStatusRequest req)
        {
            _game.PacketNotifier.NotifyQueryStatus(userId);
            return true;
        }
    }
}
