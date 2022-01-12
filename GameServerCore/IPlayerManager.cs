using System;
using System.Collections.Generic;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.NetInfo;

namespace GameServerCore
{
    public interface IPlayerManager
    {
        void AddPlayer(KeyValuePair<string, IPlayerConfig> p);
        void AddPlayer(Tuple<uint, ClientInfo> p);
        ClientInfo GetClientInfoByChampion(IChampion champ);
        ClientInfo GetPeerInfo(long playerId);
        List<Tuple<uint, ClientInfo>> GetPlayers(bool includeBots = false);
    }
}