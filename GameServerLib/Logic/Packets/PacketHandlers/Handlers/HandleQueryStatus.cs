using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public class HandleQueryStatus : PacketHandlerBase
    {
        private readonly Game _game;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_QueryStatusReq;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleQueryStatus(Game game)
        {
            _game = game;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var response = new QueryStatus();
            return _game.PacketHandlerManager.sendPacket(peer, response, Channel.CHL_S2C);
        }
    }
}
