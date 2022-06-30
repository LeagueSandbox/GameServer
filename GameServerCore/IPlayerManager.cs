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
        ClientInfo GetPeerInfo(int userId);
        ClientInfo GetClientInfoByPlayerId(long playerId);
        ClientInfo GetClientInfoByChampion(IChampion champ);
        List<ClientInfo> GetPlayers(bool includeBots = false);
    }
}