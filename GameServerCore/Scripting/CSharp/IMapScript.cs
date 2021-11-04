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
        bool HasInnerTurrets { get; }
        bool EnableBuildingProtection { get; }
        long FirstSpawnTime { get; }
        long NextSpawnTime { get; set; }
        long SpawnInterval { get; }
        //In case someone wishes to use a custom, hardcoded path instead of the one from files
        bool MinionPathingOverride { get; }
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
        Dictionary<LaneID, List<Vector2>> MinionPaths { get; }
        Tuple<int, List<MinionSpawnType>> MinionWaveToSpawn(float gameTime, int cannonMinionCount, bool isInhibitorDead, bool areAllInhibitorsDead);
        TurretType GetTurretType(int trueIndex, LaneID lane, TeamId teamId);
        void Init(IMap map);
        void SetMinionStats(ILaneMinion m);
        float GetGoldFor(IAttackableUnit u);
        float GetExperienceFor(IAttackableUnit u);
    }
}