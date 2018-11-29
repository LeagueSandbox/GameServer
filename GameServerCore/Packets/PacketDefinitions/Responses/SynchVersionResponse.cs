using GameServerCore.NetInfo;
using System.Collections.Generic;

namespace GameServerCore.Packets.PacketDefinitions.Responses
{
    public class SynchVersionResponse : ICoreResponse
    {
        public int UserId { get; }
        public List<Pair<uint, ClientInfo>> Players { get; }
        public string Version { get; }
        public string GameMode { get; }
        public int MapId { get; }
        public SynchVersionResponse(int userId, List<Pair<uint, ClientInfo>> players, string version, string gameMode, int mapId)
        {
            UserId = userId;
            Players = players;
            Version = version;
            GameMode = gameMode;
            MapId = mapId;
        }
    }
}