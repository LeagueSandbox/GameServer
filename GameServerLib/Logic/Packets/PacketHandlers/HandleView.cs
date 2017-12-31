using ENet;
using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleView : PacketHandlerBase<ViewRequest>
    {
        private readonly Game _game;

        public override PacketCmd PacketType => PacketCmd.PKT_C2S_ViewReq;
        public override Channel PacketChannel => Channel.CHL_C2S;

        public HandleView(Game game)
        {
            _game = game;
        }

        public override bool HandlePacketInternal(Peer peer, ViewRequest data)
        {
            var answer = new ViewResponse(data.NetId);
            if (data.RequestNo == 0xFE)
            {
                answer.setRequestNo(0xFF);
            }
            else
            {
                answer.setRequestNo(data.RequestNo);
            }
            _game.PacketHandlerManager.sendPacket(peer, answer, Channel.CHL_S2C, PacketFlags.None);
            return true;
        }
    }
}
