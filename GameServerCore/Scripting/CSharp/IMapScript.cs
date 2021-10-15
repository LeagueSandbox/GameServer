using System;
using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Maps;

namespace GameServerCore.Domain
{
    public interface IMapScript : IUpdate
    {
        bool HasTopLane { get; set; }
        bool HasMidLane { get; set; }
        bool HasBotLane { get; set; }
        float GoldPerSecond { get; set; }
        float StartingGold { get; set; }
        bool HasFirstBloodHappened { get; set; }
        bool IsKillGoldRewardReductionActive { get; set; }
        int BluePillId { get; set; }
        long FirstGoldTime { get; set; }
        bool SpawnEnabled { get; set; }
        void StartUp(IGame game);
        void AddFountain(TeamId team, Vector2 position);
        int[] GetTurretItems(TurretType type);
        TurretType GetTurretType(int trueIndex, LaneID lane);
        string GetTowerModel(TurretType type, TeamId teamId);
        void ChangeTowerOnMapList(string towerName, TeamId team, LaneID currentLaneId, LaneID desiredLaneID);
        void Init(IMap map);
        Tuple<TeamId, Vector2> GetMinionSpawnPosition(string barracksName);

        void SetMinionStats(ILaneMinion m);

        string GetMinionModel(TeamId team, MinionSpawnType type);

        float GetGoldFor(IAttackableUnit u);

        float GetExperienceFor(IAttackableUnit u);

        void HandleSurrender(int userId, IChampion who, bool vote);
    }
}