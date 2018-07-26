using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleQueryStatus : PacketHandlerBase
    {
        private readonly Game _game;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_QUERY_STATUS_REQ;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleQueryStatus(Game game)
        {
            _game = game;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var response = new QueryStatus(_game);
            return _game.PacketHandlerManager.SendPacket(peer, response, Channel.CHL_S2C);
        }
    }
}
