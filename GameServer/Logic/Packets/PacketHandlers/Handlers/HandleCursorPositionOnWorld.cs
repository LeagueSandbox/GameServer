using ENet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.Packets;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Core.Logic.PacketHandlers.Packets
{
    class HandleCursorPositionOnWorld : IPacketHandler
    {
        private Game _game = Program.ResolveDependency<Game>();
        private PlayerManager _playerManager = Program.ResolveDependency<PlayerManager>();

        public bool HandlePacket(Peer peer, byte[] data)
        {
            var cursorPosition = new CursorPositionOnWorld(data);
            var response = new DebugMessage(string.Format("X: {0} Y: {1}", cursorPosition.X, cursorPosition.Y));

            return _game.PacketHandlerManager.broadcastPacket(response, Channel.CHL_S2C);
        }
    }
}
