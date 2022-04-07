using GameServerCore;
using GameServerCore.Packets.Handlers;
using LeaguePackets.Game;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleSurrender : PacketHandlerBase<C2S_TeamSurrenderVote>
    {
        private readonly Game _game;
        private readonly IPlayerManager _pm;

        public HandleSurrender(Game game)
        {
            _game = game;
            _pm = game.PlayerManager;
        }

        public override bool HandlePacket(int userId, C2S_TeamSurrenderVote req)
        {
            var c = _pm.GetPeerInfo(userId).Champion;
            HandleSurrender(userId, c, req.VotedYes);
            return true;
        }
    }
}
