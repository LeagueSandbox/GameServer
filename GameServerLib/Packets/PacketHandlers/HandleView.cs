using ENet;
using GameServerCore.Packets.Enums;

namespace LeagueSandbox.GameServer.Packets.PacketHandlers
{
    public class HandleView : PacketHandlerBase
    {
        private readonly Game _game;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_VIEW_REQ;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleView(Game game)
        {
            _game = game;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = _game.PacketReader.ReadViewRequest(data);
             _game.PacketNotifier.NotifyViewResponse(peer, request);
            return true;
        }
    }
}
