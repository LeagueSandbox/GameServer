using ENet;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleView : PacketHandlerBase
    {
        private readonly IPacketReader _packetReader;
        private readonly IPacketNotifier _packetNotifier;
        private readonly Game _game;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_VIEW_REQ;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleView(Game game)
        {
            _packetReader = game.PacketReader;
            _packetNotifier = game.PacketNotifier;
            _game = game;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _packetReader.ReadViewRequest(data);
            _packetNotifier.NotifyViewResponse(peer, request);
            return true;
        }
    }
}
