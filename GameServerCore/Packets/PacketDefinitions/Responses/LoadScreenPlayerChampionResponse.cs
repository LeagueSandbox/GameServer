using GameServerCore.NetInfo;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class LoadScreenPlayerChampionResponse : ICoreResponse
    {
        public int UserId { get; }
        public Pair<uint, ClientInfo> Player { get; }
        public LoadScreenPlayerChampionResponse(int userId, Pair<uint, ClientInfo> player)
        {
            UserId = userId;
            Player = player;
        }
    }
}