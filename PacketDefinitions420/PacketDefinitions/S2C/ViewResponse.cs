using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
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