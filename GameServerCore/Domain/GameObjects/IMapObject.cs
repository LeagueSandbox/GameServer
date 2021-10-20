using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Domain
{
    public interface IMapObject
    {
        public string Name { get; }
        public Vector3 CentralPoint { get; }
        public int ParentMapId { get; }
        GameObjectTypes GetGameObjectType();
        TeamId GetTeamID();
        TeamId GetOpposingTeamID();
        string GetTeamName();
        LaneID GetLaneID();
        LaneID GetSpawnBarrackLaneID();
        int ParseIndex();
    }
}