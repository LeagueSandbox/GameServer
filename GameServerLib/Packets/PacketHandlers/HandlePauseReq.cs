using GameServerCore.Packets.PacketDefinitions.Requests;
using GameServerCore.Packets.Handlers;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandlePauseReq : PacketHandlerBase<PauseRequest>
    {
        private readonly Game _game;

        public HandlePauseReq(Game game)
        {
            _game = game;
        }

        public override bool HandlePacket(int userId, PauseRequest req)
        {
            _game.Pause();
            return true;
        }
    }
}