using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ViewResponse : Packet
    {
        public ViewResponse(uint senderNetId)
            : base(PacketCmd.PKT_S2C_ViewAns)
        {
            buffer.Write(senderNetId);
        }

        public void setRequestNo(byte requestNo)
        {
            buffer.Write(requestNo);
        }
    }
}