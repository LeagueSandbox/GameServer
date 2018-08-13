using ENet;
using GameServerCore.Packets.Enums;
using GameServerCore.Packets.Interfaces;
using LeagueSandbox.GameServer.Logic.Players;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleCursorPositionOnWorld : PacketHandlerBase
    {
        private readonly IPacketReader _packetReader;
        private readonly IPacketNotifier _packetNotifier;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_CURSOR_POSITION_ON_WORLD;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleCursorPositionOnWorld(Game game)
        {
            _packetReader = game.PacketReader;
            _packetNotifier = game.PacketNotifier;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _packetReader.ReadCursorPositionOnWorldRequest(data);
            _packetNotifier.NotifyDebugMessage($"X: {request.X} Y: {request.Y}");
            return true;
        }
    }
}
