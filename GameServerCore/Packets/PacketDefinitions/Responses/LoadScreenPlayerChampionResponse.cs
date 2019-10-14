using GameServerCore.NetInfo;
using System;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class LoadScreenPlayerChampionResponse : ICoreResponse
    {
        public int UserId { get; }
        public Tuple<uint, ClientInfo> Player { get; }
        public LoadScreenPlayerChampionResponse(int userId, Tuple<uint, ClientInfo> player)
        {
            UserId = userId;
            Player = player;
        }
    }
}