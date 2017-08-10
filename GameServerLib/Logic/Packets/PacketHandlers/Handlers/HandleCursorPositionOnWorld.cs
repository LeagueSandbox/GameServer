using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public class HandleCursorPositionOnWorld : PacketHandlerBase
    {
        private readonly Game _game;
        private readonly PlayerManager _playerManager;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CursorPositionOnWorld;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleCursorPositionOnWorld(Game game, PlayerManager playerManager)
        {
            _game = game;
            _playerManager = playerManager;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var cursorPosition = new CursorPositionOnWorld(data);
            var response = new DebugMessage($"X: {cursorPosition.X} Y: {cursorPosition.Y}");

            return _game.PacketHandlerManager.broadcastPacket(response, Channel.CHL_S2C);
        }
    }
}
