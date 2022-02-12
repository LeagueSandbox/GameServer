using GameServerCore.Domain;
using GameServerCore.Enums;
using System.Collections.Generic;
using System.Numerics;
using static GameServerLib.API.APIMapFunctionManager;

namespace MapScripts.Map12
{
    public class NeutralMinionSpawn
    {
        private static bool forceSpawn;

        public static List<IMonsterCamp> HealthPacks = new List<IMonsterCamp>();

        public static void InitializeJungle()
        {
            var purple_healthPacket1 = CreateJungleCamp(new Vector3(7582.1f, 60.0f, 6785.5f), 1, TeamId.TEAM_NEUTRAL, "HealthPack", 190.0f * 1000f);
            CreateJungleMonster("HA_AP_HealthRelic1.1.1", "HA_AP_HealthRelic", new Vector2(7582.1f, 6785.5f), new Vector3(7582.1f, -193.8f, 6785.5f), purple_healthPacket1);
            HealthPacks.Add(purple_healthPacket1);

            var blue_healthPacket1 = CreateJungleCamp(new Vector3(5929.7f, 60.0f, 5190.9f), 2, TeamId.TEAM_NEUTRAL, "HealthPack", 190.0f * 1000f);
            CreateJungleMonster("HA_AP_HealthRelic2.1.1", "HA_AP_HealthRelic", new Vector2(5929.7f, 5190.9f), new Vector3(5929.7f, -194.0f, 5190.9f), blue_healthPacket1);
            HealthPacks.Add(blue_healthPacket1);

            var purple_healthPacket2 = CreateJungleCamp(new Vector3(8893.9f, 60.0f, 7889.0f), 3, TeamId.TEAM_NEUTRAL, "HealthPack", 190.0f * 1000f);
            CreateJungleMonster("HA_AP_HealthRelic3.1.1", "HA_AP_HealthRelic", new Vector2(8893.9f, 7889.0f), new Vector3(8893.9f, -187.7f, 7889.0f), purple_healthPacket2);
            HealthPacks.Add(purple_healthPacket2);

            var blue_healthPacket2 = CreateJungleCamp(new Vector3(4790.2f, 60.0f, 3934.3f), 4, TeamId.TEAM_NEUTRAL, "HealthPack", 190.0f * 1000f);
            CreateJungleMonster("HA_AP_HealthRelic4.1.1", "HA_AP_HealthRelic", new Vector2(4790.2f, 3934.3f), new Vector3(4790.2f, -188.5f, 3934.3f), blue_healthPacket2);
            HealthPacks.Add(blue_healthPacket2);
        }

        public static void OnUpdate(float diff)
        {
            foreach (var camp in HealthPacks)
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

        public static void ForceCampSpawn()
        {
            forceSpawn = true;
        }
    }
}
