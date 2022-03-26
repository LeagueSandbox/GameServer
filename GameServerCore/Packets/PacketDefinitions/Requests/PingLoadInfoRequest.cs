using LeaguePackets;
using LeaguePackets.Game;

namespace GameServerCore.Packets.PacketDefinitions.Requests
{
    public class PingLoadInfoRequest : ICoreRequest
    {
        public C2S_Ping_Load_Info InfoRequest = new C2S_Ping_Load_Info();

        public PingLoadInfoRequest(byte[] data)
        {
            InfoRequest.Read(data);
        }
    }
}
