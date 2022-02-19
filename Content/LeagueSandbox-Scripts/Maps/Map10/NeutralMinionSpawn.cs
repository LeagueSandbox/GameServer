using GameServerCore.Domain;
using GameServerCore.Enums;
using System.Collections.Generic;
using System.Numerics;
using static GameServerLib.API.APIMapFunctionManager;

namespace MapScripts.Map10
{
    public class NeutralMinionSpawn
    {
        private static bool forceSpawn;

        public static List<IMonsterCamp> MonsterCamps = new List<IMonsterCamp>();

        public static void InitializeCamps()
        {
            //Blue Side Wraiths
            var blue_Wraiths = CreateJungleCamp(new Vector3(4414.48f, 60.0f, 5774.88f), groupNumber: 1, teamSideOfTheMap: TeamId.TEAM_BLUE, campTypeIcon: "Camp", 100.0f * 1000);
            CreateJungleMonster("TT_NWraith1.1.1", "TT_NWraith", new Vector2(4414.48f, 5774.88f), new Vector3(4214.47f, -109.177f, 5962.65f), blue_Wraiths, aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("TT_NWraith21.1.2", "TT_NWraith2", new Vector2(4247.32f, 5725.39f), new Vector3(4214.47f, -109.177f, 5962.65f), blue_Wraiths, aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("TT_NWraith21.1.3", "TT_NWraith2", new Vector2(4452.47f, 5909.56f), new Vector3(4214.47f, -109.177f, 5962.65f), blue_Wraiths, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(blue_Wraiths);

            //Blue Side Golems
            var blue_Golems = CreateJungleCamp(new Vector3(5088.37f, 60.0f, 8065.55f), groupNumber: 2, teamSideOfTheMap: TeamId.TEAM_BLUE, campTypeIcon: "Camp", 100.0f * 1000);
            CreateJungleMonster("TT_NGolem2.1.1", "TT_NGolem", new Vector2(5088.37f, 8065.55f), new Vector3(4861.72f, -109.332f, 7825.94f), blue_Golems, aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("TT_NGolem22.1.2", "TT_NGolem2", new Vector2(5176.61f, 7810.42f), new Vector3(4861.72f, -109.332f, 7825.94f), blue_Golems, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(blue_Golems);

            //Blue Side Wolves
            var blue_Wolves = CreateJungleCamp(new Vector3(6148.92f, 60.0f, 5993.49f), groupNumber: 3, teamSideOfTheMap: TeamId.TEAM_BLUE, campTypeIcon: "Camp", 100.0f * 1000);
            CreateJungleMonster("TT_NWolf3.1.1", "TT_NWolf", new Vector2(6148.92f, 5993.49f), new Vector3(5979.61f, -109.744f, 6236.2f), blue_Wolves, aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("TT_NWolf23.1.2", "TT_NWolf2", new Vector2(6010.29f, 6010.79f), new Vector3(5979.61f, -109.744f, 6236.2f), blue_Wolves, aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("TT_NWolf23.1.3", "TT_NWolf2", new Vector2(6202.73f, 6156.5f), new Vector3(5979.61f, -109.744f, 6236.2f), blue_Wolves, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(blue_Wolves);

            //Red Side Wraiths
            var red_Wraiths = CreateJungleCamp(new Vector3(11008.2f, 60.0f, 5775.7f), groupNumber: 4, teamSideOfTheMap: TeamId.TEAM_PURPLE, campTypeIcon: "Camp", 100.0f * 1000);
            CreateJungleMonster("TT_NWraith4.1.1", "TT_NWraith", new Vector2(11008.2f, 5775.7f), new Vector3(11189.8f, -109.202f, 5939.67f), red_Wraiths, aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("TT_NWraith24.1.2f", "TT_NWraith2", new Vector2(10953.2f, 5919.11f), new Vector3(11189.8f, -109.202f, 5939.67f), red_Wraiths, aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("TT_NWraith24.1.3", "TT_NWraith2", new Vector2(11168.8f, 5695.25f), new Vector3(11189.8f, -109.202f, 5939.67f), red_Wraiths, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(red_Wraiths);

            //Red Side Golems
            var red_Golems = CreateJungleCamp(new Vector3(10341.3f, 60.0f, 8084.77f), groupNumber: 5, teamSideOfTheMap: TeamId.TEAM_PURPLE, campTypeIcon: "Camp", 100.0f * 1000);
            CreateJungleMonster("TT_NGolem5.1.1f", "TT_NGolem", new Vector2(10341.3f, 8084.77f), new Vector3(10433.8f, -109.466f, 7930.07f), red_Golems, aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("TT_NGolem25.1.2", "TT_NGolem2", new Vector2(10256.8f, 7842.84f), new Vector3(10433.8f, -109.466f, 7930.07f), red_Golems, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(red_Golems);

            //Red Side Wolves
            var red_Wolves = CreateJungleCamp(new Vector3(9239.0f, 60.0f, 6022.87f), groupNumber: 6, teamSideOfTheMap: TeamId.TEAM_PURPLE, campTypeIcon: "Camp", 100.0f * 1000);
            CreateJungleMonster("TT_NWolf6.1.1", "TT_NWolf", new Vector2(9239.0f, 6022.87f), new Vector3(9411.97f, -109.837f, 6214.06f), red_Wolves, aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("TT_NWolf26.1.2", "TT_NWolf2", new Vector2(9186.8f, 6176.57f), new Vector3(9411.97f, -109.837f, 6214.06f), red_Wolves, aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("TT_NWolf26.1.3", "TT_NWolf2", new Vector2(9404.52f, 5996.73f), new Vector3(9411.97f, -109.837f, 6214.06f), red_Wolves, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(red_Wolves);

            //Center of the Map Health Pack
            var healthPack = CreateJungleCamp(new Vector3(7711.15f, 60.0f, 6722.67f), groupNumber: 7, teamSideOfTheMap: 0, campTypeIcon: "HealthPack", 115.0f * 1000);
            CreateJungleMonster("TT_Relic7.1.1", "TT_Relic", new Vector2(7711.15f, 6722.67f), new Vector3(7711.15f, -112.716f, 6322.67f), healthPack);
            MonsterCamps.Add(healthPack);

            //Vilemaw
            //TODO: VIle maw needs it's own Special A.I Script, for now it'll be just a dummy.
            var spiderBoss = CreateJungleCamp(new Vector3(7711.15f, 60.0f, 10080.0f), groupNumber: 8, teamSideOfTheMap: 0, campTypeIcon: "Epic", 600.0f * 1000);
            CreateJungleMonster("TT_Spiderboss8.1.1", "TT_Spiderboss", new Vector2(7711.15f, 10080.0f), new Vector3(7726.41f, -108.603f, 9234.69f), spiderBoss);
            MonsterCamps.Add(spiderBoss);
        }

        public static void OnUpdate(float diff)
        {
            foreach (var camp in MonsterCamps)
            {
                if (!camp.IsAlive)
                {
                    camp.RespawnTimer -= diff;
                    if (camp.RespawnTimer <= 0 || forceSpawn)
                    {
                        SpawnCamp(camp);
                        camp.RespawnTimer = GetRespawnTimer(camp);
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

        public static float GetRespawnTimer(IMonsterCamp monsterCamp)
        {
            switch (monsterCamp.CampIndex)
            {
                case 7:
                    return 90.0f * 1000;
                case 8:
                    return 300.0f * 1000;
                default:
                    return 50.0f * 1000;
            }
        }
    }
}
