using GameServerCore.Packets.Handlers;
using GameServerCore.Packets.PacketDefinitions.Requests;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleCursorPositionOnWorld : PacketHandlerBase<CursorPositionOnWorldRequest>
    {
        private readonly Game _game;

        public HandleCursorPositionOnWorld(Game game)
        {
            _game = game;
        }

        public override bool HandlePacket(int userId, CursorPositionOnWorldRequest req)
        {
            _game.PacketNotifier.NotifyDebugMessage($"X: {req.X} Y: {req.Y}");
            return true;
        }
    }
}
