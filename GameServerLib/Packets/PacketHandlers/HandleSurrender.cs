using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Packets.Handlers;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;
using LeagueSandbox.GameServer.Players;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSurrender : PacketHandlerBase<SurrenderRequest>
    {
        private readonly Game _game;
        private readonly PlayerManager _pm;

        public HandleSurrender(Game game)
        {
            _game = game;
            _pm = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, SurrenderRequest req)
        {
            var c = _pm.GetPeerInfo(userId).Champion;
            HandleSurrender(userId, c, req.VotedYes);
            return true;
        }
    }
}
