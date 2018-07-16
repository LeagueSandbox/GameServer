using LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.C2S;
using LeagueSandbox.GameServer.Logic.Packets.PacketHandlers;

namespace LeagueSandbox.GameServer.Logic.Packets.PacketDefinitions.S2C
{
    public class ViewResponse : Packet
    {
        public ViewResponse(Game game, ViewRequest request)
            : base(game, PacketCmd.PKT_S2C_VIEW_ANS)
        {
            Write(request.NetId);
        }

        public void SetRequestNo(byte requestNo)
        {
            Write(requestNo);
        }
    }
}