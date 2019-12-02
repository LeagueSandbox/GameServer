using GameServerCore.NetInfo;
using System;
using System.Collections.Generic;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class LoadScreenInfoResponse : ICoreResponse
    {
        public int UserId { get; }
        public List<Tuple<uint, ClientInfo>> Players { get; }
        public LoadScreenInfoResponse(int userId, List<Tuple<uint, ClientInfo>> players)
        {
            UserId = userId;
            Players = players;
        }
    }
}