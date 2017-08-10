using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers.Handlers
{
    public class HandleView : PacketHandlerBase
    {
        private readonly Game _game;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_ViewReq;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleView(Game game)
        {
            _game = game;
        }

        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = new ViewRequest(data);
            var answer = new ViewAnswer(request);
            if (request.requestNo == 0xFE)
            {
                answer.setRequestNo(0xFF);
            }
            else
            {
                answer.setRequestNo(request.requestNo);
            }
            _game.PacketHandlerManager.sendPacket(peer, answer, Channel.CHL_S2C, PacketFlags.None);
            return true;
        }
    }
}
