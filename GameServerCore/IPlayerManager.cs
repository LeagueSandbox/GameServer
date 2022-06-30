using System;
using System.Collections.Generic;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.NetInfo;

namespace GameServerCore
{
    public interface IPlayerManager
    {
        void AddPlayer(IPlayerConfig config);
        void AddPlayer(ClientInfo info);
        ClientInfo GetClientInfoByChampion(IChampion champ);
        ClientInfo GetPeerInfo(int userId);
        List<ClientInfo> GetPlayers(bool includeBots = false);
    }
}