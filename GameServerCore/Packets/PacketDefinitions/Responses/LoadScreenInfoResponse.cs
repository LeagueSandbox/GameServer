using GameServerCore.NetInfo;
using System.Collections.Generic;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class LoadScreenInfoResponse : ICoreResponse
    {
        public int UserId { get; }
        public List<Pair<uint, ClientInfo>> Players { get; }
        public LoadScreenInfoResponse(int userId, List<Pair<uint, ClientInfo>> players)
        {
            UserId = userId;
            Players = players;
        }
    }
}