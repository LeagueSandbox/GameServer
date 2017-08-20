using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Scripting.CSharp
{
    public interface MapGameScript
    {
        List<int> ExpToLevelUp { get; set; }
        float GoldPerSecond { get; set; }
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

        float GetGoldFor(Unit u);

        float GetExperienceFor(Unit u);

        float[] GetEndGameCameraPosition(TeamId team);
    }
}
