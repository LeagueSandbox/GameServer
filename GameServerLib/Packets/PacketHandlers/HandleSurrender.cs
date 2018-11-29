using GameServerCore;
using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSurrender : PacketHandlerBase<SurrenderRequest>
    {
        private readonly Game _game;
        private readonly IPlayerManager _pm;

        public HandleSurrender(Game game)
        {
            _game = game;
            _pm = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, SurrenderRequest req)
        {
            var c = _pm.GetPeerInfo(userId).Champion;
             _game.PacketNotifier.NotifySurrender(c, 0x03, 1, 0, 5, c.Team, 10.0f);
            return true;
        }
    }
}
