using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Packets.Handlers;

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
            _game.PacketNotifier.NotifyS2C_QueryStatusAns(userId);
            return true;
        }
    }
}
