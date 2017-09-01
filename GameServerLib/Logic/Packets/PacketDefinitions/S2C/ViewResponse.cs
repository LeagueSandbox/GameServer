using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ViewResponse : Packet
    {
        public ViewResponse(ViewRequest request)
            : base(PacketCmd.PKT_S2C_ViewAns)
        {
            buffer.Write(request.netId);
        }

        public void setRequestNo(byte requestNo)
        {
            buffer.Write(requestNo);
        }
    }
}