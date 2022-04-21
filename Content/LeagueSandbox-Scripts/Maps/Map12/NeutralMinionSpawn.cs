using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using System.Collections.Generic;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;

namespace MapScripts.Map12
{
    public class NeutralMinionSpawn
    {
        private static bool forceSpawn;

        public static Dictionary<IMonsterCamp, IMonster> HealthPacks = new Dictionary<IMonsterCamp, IMonster>();
        private static Vector3[]  _champPositions = {
            new Vector3(7582.1f, 60.0f, 6785.5f),
            new Vector3(5929.7f, 60.0f, 5190.9f),
            new Vector3(8893.9f, 60.0f, 7889.0f),
            new Vector3(4790.2f, 60.0f, 3934.3f),
        };

        public static void InitializeJungle()
        {
            for(byte i = 1; i <= _champPositions.Length; i++)
            {
                Vector3 pos = _champPositions[i - 1];
                var camp = CreateJungleCamp(pos, i, TeamId.TEAM_NEUTRAL, "HealthPack", 190.0f * 1000f);
                var monster = CreateJungleMonster(
                    $"HA_AP_HealthRelic{i}.1.1", "HA_AP_HealthRelic",
                    new Vector2(pos.X, pos.Z), Vector3.Zero,
                    camp, isTargetable: false
                );
                HealthPacks.Add(camp, monster);
            }
        }

        public static void OnUpdate(float diff)
        {
            foreach (var camp in HealthPacks.Keys)
            {
                if (!camp.IsAlive)
                {
                    camp.RespawnTimer -= diff;
                    if (camp.RespawnTimer <= 0 || forceSpawn)
                    {
                        SpawnCamp(camp);
                        camp.RespawnTimer = 40.0f * 1000f;
                    }
                }
            }

            if (forceSpawn)
            {
                forceSpawn = false;
            }
        }

        public static void SpawnCamp(IMonsterCamp monsterCamp)
        {
           monsterCamp.AddMonster(HealthPacks[monsterCamp]);
        }

        public static void ForceCampSpawn()
        {
            forceSpawn = true;
        }
    }
}
