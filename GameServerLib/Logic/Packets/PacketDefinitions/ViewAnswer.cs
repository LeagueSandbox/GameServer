using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets
{
    public class ViewAnswer : Packet
    {
        public ViewAnswer(ViewRequest request) : base(PacketCmd.PKT_S2C_ViewAns)
        {
            buffer.Write(request.netId);
        }
        public void setRequestNo(byte requestNo)
        {
            buffer.Write(requestNo);
        }
    }
}