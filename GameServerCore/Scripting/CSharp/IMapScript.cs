using System;
using System.Collections.Generic;
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
        Dictionary<TurretType, int[]> TurretItems { get; }
        Dictionary<TeamId, Dictionary<TurretType, string>> TowerModels { get; }
        Dictionary<TeamId, Dictionary<MinionSpawnType, string>> MinionModels { get; }
        TurretType GetTurretType(int trueIndex, LaneID lane);
        void Init(IMap map);
        void SetMinionStats(ILaneMinion m);

        float GetGoldFor(IAttackableUnit u);

        float GetExperienceFor(IAttackableUnit u);

        void HandleSurrender(int userId, IChampion who, bool vote);
    }
}