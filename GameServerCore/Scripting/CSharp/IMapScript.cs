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
        bool HasTopLane { get; }
        bool HasMidLane { get; }
        bool HasBotLane { get; }
        long FirstSpawnTime { get; }
        long NextSpawnTime { get; set; }
        long SpawnInterval { get; }
        float GoldPerSecond { get; }
        float StartingGold { get; }
        bool HasFirstBloodHappened { get; set; }
        bool IsKillGoldRewardReductionActive { get; set; }
        int BluePillId { get; }
        long FirstGoldTime { get; }
        bool SpawnEnabled { get; set; }
        Dictionary<TurretType, int[]> TurretItems { get; }
        Dictionary<TeamId, Dictionary<TurretType, string>> TowerModels { get; }
        Dictionary<TeamId, Dictionary<MinionSpawnType, string>> MinionModels { get; }
        public Dictionary<TeamId, Dictionary<LaneID, List<Vector2>>> MinionPaths { get; set; }
        Tuple<int, List<MinionSpawnType>> MinionWaveToSpawn(float gameTime, int cannonMinionCount, bool isInhibitorDead, bool areAllInhibitorsDead);
        TurretType GetTurretType(int trueIndex, LaneID lane);
        void Init(IMap map);
        void SetMinionStats(ILaneMinion m);
        float GetGoldFor(IAttackableUnit u);
        float GetExperienceFor(IAttackableUnit u);
    }
}