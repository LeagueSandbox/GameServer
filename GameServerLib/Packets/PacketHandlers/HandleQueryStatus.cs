using GameServerCore.Packets.Handlers;
using LeaguePackets.Game;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleQueryStatus : PacketHandlerBase<C2S_QueryStatusReq>
    {
        private readonly Game _game;

        public HandleQueryStatus(Game game)
        {
            _game = game;
        }

        public override bool HandlePacket(int userId, C2S_QueryStatusReq req)
        {
            _game.PacketNotifier.NotifyS2C_QueryStatusAns(userId);
            return true;
        }
    }
}
