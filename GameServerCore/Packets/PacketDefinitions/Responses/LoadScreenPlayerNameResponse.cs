using GameServerCore.Domain;
using GameServerCore.NetInfo;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class LoadScreenPlayerNameResponse : ICoreResponse
    {
        public int UserId { get; }
        public Pair<uint, ClientInfo> Player { get; }
        public LoadScreenPlayerNameResponse(int userId, Pair<uint, ClientInfo> player)
        {
            UserId = userId;
            Player = player;
        }
    }
}