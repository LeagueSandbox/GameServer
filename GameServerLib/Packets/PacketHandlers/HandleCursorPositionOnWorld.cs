using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Handlers;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleCursorPositionOnWorld : PacketHandlerBase
    {
        private readonly Game _game;
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CURSOR_POSITION_ON_WORLD;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleCursorPositionOnWorld(Game game)
        {
            _game = game;
        }

        public override bool HandlePacket(int userId, byte[] data)
        {
            var request = _game.PacketReader.ReadCursorPositionOnWorldRequest(data);
            _game.PacketNotifier.NotifyDebugMessage($"X: {request.X} Y: {request.Y}");
            return true;
        }
    }
}
