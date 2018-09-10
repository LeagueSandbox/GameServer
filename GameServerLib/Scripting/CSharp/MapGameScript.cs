using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.Other;

namespace LeagueSandbox.GameServer.Scripting.CSharp
{
    public interface IMapGameScript
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
        void Update(float diff);

        Tuple<TeamId, Vector2> GetMinionSpawnPosition(MinionSpawnPosition spawnPosition);

        void SetMinionStats(Minion m);

        Target GetRespawnLocation(TeamId team);

        string GetMinionModel(TeamId team, MinionSpawnType type);

        float GetGoldFor(AttackableUnit u);

        float GetExperienceFor(AttackableUnit u);

        Vector3 GetEndGameCameraPosition(TeamId team);
    }
}
