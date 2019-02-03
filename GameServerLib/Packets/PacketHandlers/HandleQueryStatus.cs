using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleQueryStatus : PacketHandlerBase<QueryStatusRequest>
    {
        private readonly Game _game;

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
