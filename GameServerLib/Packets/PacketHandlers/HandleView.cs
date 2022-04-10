using GameServerCore.Packets.Handlers;
using LeaguePackets.Game;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleView : PacketHandlerBase<World_SendCamera_Server>
    {
        private readonly Game _game;

        public HandleView(Game game)
        {
            _game = game;
        }

        public override bool HandlePacket(int userId, World_SendCamera_Server req)
        {
             _game.PacketNotifier.NotifyWorld_SendCamera_Server_Acknologment(userId, req);
            return true;
        }
    }
}
