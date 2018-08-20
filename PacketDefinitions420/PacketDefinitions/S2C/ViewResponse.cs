using GameServerCore.Packets.Enums;
using GameServerCore.Packets.PacketDefinitions;

namespace PacketDefinitions420.PacketDefinitions.S2C
{
    public class ViewResponse : Packet
    {
        public ViewResponse(int netId)
            : base(PacketCmd.PKT_S2C_VIEW_ANS)
        {
            Write(netId);
        }

        public void SetRequestNo(byte requestNo)
        {
            Write(requestNo);
        }
    }
}