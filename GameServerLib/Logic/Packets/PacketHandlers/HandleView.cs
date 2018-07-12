using ENet;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketHandlers
{
    public class HandleView : PacketHandlerBase
    {
        public override PacketCmd PacketType => PacketCmd.PKT_C2S_VIEW_REQ;
        public override Channel PacketChannel => Channel.CHL_C2_S;
        public override bool HandlePacket(Peer peer, byte[] data)
        {
            var request = new ViewRequest(data);
            var answer = new ViewResponse(request);
            if (request.RequestNo == 0xFE)
            {
                answer.SetRequestNo(0xFF);
            }
            else
            {
                answer.SetRequestNo(request.RequestNo);
            }
            Game.PacketHandlerManager.SendPacket(peer, answer, Channel.CHL_S2_C, PacketFlags.None);
            return true;
        }
    }
}
