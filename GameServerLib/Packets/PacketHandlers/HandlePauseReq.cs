using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;
using LeaguePackets.Game;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandlePauseReq : PacketHandlerBase<PausePacket>
    {
        private readonly Game _game;

        public HandlePauseReq(Game game)
        {
            _game = game;
        }

        public override bool HandlePacket(int userId, PausePacket req)
        {
            _game.Pause();
            return true;
        }
    }
}