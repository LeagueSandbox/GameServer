using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandlePauseReq : PacketHandlerBase
    {
        private readonly Game _game;

        public override PacketCmd PacketType => PacketCmd.PKT_PAUSE_GAME;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandlePauseReq(Game game)
        {
            _game = game;
        }

        public override bool HandlePacket(int userId, byte[] data)
        {
            _game.Pause();
            return true;
        }
    }
}