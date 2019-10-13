using System;
using System.Collections.Generic;
using GameServerCore.Domain.GameObjects;
using GameServerCore.NetInfo;

namespace GameServerCore
{
    public interface IPlayerManager
    {
        ClientInfo GetClientInfoByChampion(IChampion champ);
        ClientInfo GetPeerInfo(ulong playerId);
        List<Tuple<uint, ClientInfo>> GetPlayers();
    }
}