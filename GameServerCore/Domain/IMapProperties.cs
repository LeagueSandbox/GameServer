using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface IMapProperties: IUpdate
    {
        List<int> ExpToLevelUp { get; set; }
        float GoldPerSecond { get; set; }
        float StartingGold { get; set; }
        bool HasFirstBloodHappened { get; set; }
        bool IsKillGoldRewardReductionActive { get; set; }
        int BluePillId { get; set; }
        long FirstGoldTime { get; set; }
        bool SpawnEnabled { get; set; }

        void Init();

        Tuple<TeamId, Vector2> GetMinionSpawnPosition(MinionSpawnPosition spawnPosition);

        void SetMinionStats(ILaneMinion m);

        ITarget GetRespawnLocation(TeamId team);

        string GetMinionModel(TeamId team, MinionSpawnType type);

        float GetGoldFor(IAttackableUnit u);

        float GetExperienceFor(IAttackableUnit u);

        Vector3 GetEndGameCameraPosition(TeamId team);
    }
}