using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleCursorPositionOnWorld : PacketHandlerBase<CursorPositionOnWorld>
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

        public override bool HandlePacketInternal(Peer peer, CursorPositionOnWorld data)
        {
            var response = new DebugMessage($"X: {data.X} Y: {data.Y}");
            return _game.PacketHandlerManager.broadcastPacket(response, Channel.CHL_S2C);
        }
    }
}
