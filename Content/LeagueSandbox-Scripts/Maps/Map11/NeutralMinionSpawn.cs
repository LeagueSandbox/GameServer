using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Maps;
using LeagueSandbox.GameServer.API;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static GameServerLib.API.APIMapFunctionManager;

namespace MapScripts.Map11
{
    public class NeutralMinionSpawn
    {
        private static bool forceSpawn;

        public static List<IMonsterCamp> MonsterCamps = new List<IMonsterCamp>();

        public static void InitializeCamps()
        {
            //Blue Side Blue Buff
            var blue_blueBuff = CreateJungleCamp(new Vector3(3871.4885f, 60.0f, 7901.054f), 1, TeamId.TEAM_BLUE, "Camp", 115.0f * 1000, spawnDuration: 1.1f);
            CreateJungleMonster("SRU_Blue1.1.1", "SRU_Blue", new Vector2(3871.4885f, 7901.054f), new Vector3(3420.6084f, 55.0703f, 8190.704f), blue_blueBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_BlueMini1.1.2", "SRU_BlueMini", new Vector2(3777.6985f, 8103.474f), new Vector3(3420.6084f, 55.0703f, 8190.704f), blue_blueBuff, spawnAnimation: "Spawn", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_BlueMini21.1.3", "SRU_BlueMini2", new Vector2(3627.9185f, 7838.854f), new Vector3(3420.6084f, 55.0703f, 8190.704f), blue_blueBuff, spawnAnimation: "Spawn", aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(blue_blueBuff);

            //Blue side Wolfs
            var blueWolves = CreateJungleCamp(new Vector3(3780.6284f, 60.0f, 6443.984f), 2, TeamId.TEAM_BLUE, "LesserCamp", 115.0f * 1000, spawnDuration: 0.66f);
            CreateJungleMonster("SRU_Murkwolf2.1.1", "SRU_Murkwolf", new Vector2(3780.6284f, 6443.984f), new Vector3(3700.6284f, 46.0f, 6385.984f), blueWolves, spawnAnimation: "Spawn", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_MurkwolfMini2.1.2", "SRU_MurkwolfMini", new Vector2(3980.6284f, 6443.984f), new Vector3(3758.9185f, 55.6167f, 6225.844f), blueWolves, spawnAnimation: "spawn_mini1", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_MurkwolfMini2.1.3", "SRU_MurkwolfMini", new Vector2(3730.6284f, 6593.984f), new Vector3(3540.3684f, 55.6125f, 6472.314f), blueWolves, spawnAnimation: "spawn_mini2", aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(blueWolves);

            //Blue Side Wraiths
            var blueWraiths = CreateJungleCamp(new Vector3(6706.678f, 60.0f, 5521.044f), 3, TeamId.TEAM_BLUE, "LesserCamp", 115.0f * 1000, spawnDuration: 1.0f);
            CreateJungleMonster("SRU_Razorbeak3.1.1", "SRU_Razorbeak", new Vector2(6706.678f, 5521.044f), new Vector3(6958.6284f, 48.0f, 5460.984f), blueWraiths, spawnAnimation: "Spawn", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_RazorbeakMini3.1.2", "SRU_RazorbeakMini", new Vector2(6929.6284f, 5647.934f), new Vector3(6949.2583f, 56.8276f, 5549.064f), blueWraiths, spawnAnimation: "Spawn4", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_RazorbeakMini3.1.3", "SRU_RazorbeakMini", new Vector2(7060.4585f, 5499.274f), new Vector3(6958.6284f, 48.0f, 5460.984f), blueWraiths, spawnAnimation: "Spawn2", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_RazorbeakMini3.1.4", "SRU_RazorbeakMini", new Vector2(6914.7183f, 5325.354f), new Vector3(6989.5435f, 50.27832f, 5328.8696f),blueWraiths, spawnAnimation: "Spawn3", aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(blueWraiths);

            //Blue Side RedBuff
            var blue_RedBuff = CreateJungleCamp(new Vector3(7862.2437f, 60.0f, 4111.1865f), 4, TeamId.TEAM_BLUE, "Camp", 115.0f * 1000, spawnDuration: 1.66f);
            CreateJungleMonster("SRU_Red4.1.1", "SRU_Red", new Vector2(7862.2437f, 4111.1865f), new Vector3(7622.3887f, 54.426f, 3947.194f), blue_RedBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_RedMini4.1.2", "SRU_RedMini", new Vector2(7574.1284f, 4161.374f), new Vector3(7636.5684f, 57.4629f, 3896.934f), blue_RedBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_RedMini4.1.3", "SRU_RedMini", new Vector2(7918.5083f, 3880.834f), new Vector3(7636.5684f, 57.4629f, 3896.934f), blue_RedBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(blue_RedBuff);

            //Blue Side Golems
            var blueGolems = CreateJungleCamp(new Vector3(8323.471f, 60.0f, 2754.9475f), 5, TeamId.TEAM_BLUE, "LesserCamp", 115.0f * 1000, spawnDuration: 0.86f);
            CreateJungleMonster("SRU_KrugMini5.1.1", "SRU_KrugMini", new Vector2(8323.471f, 2754.9475f), new Vector3(8319.629f, 45.0f, 2641.9841f), blueGolems, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_Krug5.1.2", "SRU_Krug", new Vector2(8532.471f, 2737.9475f), new Vector3(8569.629f, 45.0f, 2633.9841f), blueGolems, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(blueGolems);

            //Dragon
            var dragon = CreateJungleCamp(new Vector3(9866.148f, 60.0f, 4414.014f), 6, 0, "Dragon", 150.0f * 1000, revealEvent: 43, spawnDuration: 6.5f);
            CreateJungleMonster("Dragon6.1.1", "SRU_Dragon", new Vector2(9866.148f, 4414.014f), new Vector3(10517.629f, -67.0f, 5171.984f), dragon, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(dragon);

            //Red Side BlueBuff
            var red_BlueBuff = CreateJungleCamp(new Vector3(10931.729f, 60.0f, 6990.844f), 7, TeamId.TEAM_PURPLE, "Camp", 115.0f * 1000, spawnDuration: 1.1f);
            CreateJungleMonster("SRU_Blue7.1.1", "SRU_Blue", new Vector2(10931.729f, 6990.844f), new Vector3(11428.629f, 54.8568f, 6740.704f), red_BlueBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_BlueMini7.1.2", "SRU_BlueMini", new Vector2(11210.328f, 7064.444f), new Vector3(11428.629f, 54.8568f, 6740.704f), red_BlueBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_BlueMini27.1.3", "SRU_BlueMini2", new Vector2(11097.629f, 6789.684f), new Vector3(11428.629f, 54.8568f, 6740.704f), red_BlueBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(red_BlueBuff);

            //Red side Wolfs
            var redWolves = CreateJungleCamp(new Vector3(11008.152f, 60.0f, 8387.408f), 8, TeamId.TEAM_PURPLE, "LesserCamp", 115.0f * 1000, spawnDuration: 0.66f);
            CreateJungleMonster("SRU_Murkwolf8.1.1", "SRU_Murkwolf", new Vector2(11008.152f, 8387.408f), new Vector3(11127.629f, 53.0f, 8502.984f), redWolves, spawnAnimation: "Spawn", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_MurkwolfMini8.1.2", "SRU_MurkwolfMini", new Vector2(11058.152f, 8217.408f), new Vector3(11285.828f, 60.8542f, 8381.714f), redWolves, spawnAnimation: "spawn_mini2", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_MurkwolfMini8.1.3", "SRU_MurkwolfMini", new Vector2(10808.152f, 8387.408f), new Vector3(11024.729f, 64.4202f, 8585.544f), redWolves, spawnAnimation: "spawn_mini1", aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(redWolves);

            //Red Side Wraiths
            var redWraiths = CreateJungleCamp(new Vector3(7986.9966f, 60.0f, 9471.389f), 9, TeamId.TEAM_PURPLE, "LesserCamp", 115.0f * 1000, spawnDuration: 0.67f);
            CreateJungleMonster("SRU_Razorbeak9.1.1", "SRU_Razorbeak", new Vector2(7986.9966f, 9471.389f), new Vector3(7901.6284f, 46.0f, 9479.984f), redWraiths, spawnAnimation: "North_Spawn", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_RazorbeakMini9.1.2", "SRU_RazorbeakMini", new Vector2(7886.9966f, 9312.39f), new Vector3(7901.6284f, 46.0f, 9479.984f), redWraiths, spawnAnimation: "North_Spawn4", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_RazorbeakMini9.1.3", "SRU_RazorbeakMini", new Vector2(7756.9966f, 9451.389f), new Vector3(7795.698f, 52.3425f, 9491.014f), redWraiths, spawnAnimation: "North_Spawn2", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_RazorbeakMini9.1.4", "SRU_RazorbeakMini", new Vector2(7854.3887f, 9610.474f), new Vector3(7849.2285f, 52.1437f, 9644.084f), redWraiths, spawnAnimation: "North_Spawn3", aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(redWraiths);

            //Red Side RedBuff
            var red_RedBuff = CreateJungleCamp(new Vector3(7016.869f, 60.0f, 10775.547f), 10, TeamId.TEAM_PURPLE, "Camp", 115.0f * 1000, spawnDuration: 1.66f);
            CreateJungleMonster("SRU_Red10.1.1", "SRU_Red", new Vector2(7016.869f, 10775.547f), new Vector3(7257.2285f, 56.4084f, 11071.284f), red_RedBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_RedMini10.1.2", "SRU_RedMini", new Vector2(7294.6387f, 10795.784f), new Vector3(7113.1084f, 54.4883f, 11017.484f), red_RedBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_RedMini10.1.3", "SRU_RedMini", new Vector2(6907.0884f, 11041.684f), new Vector3(7113.1084f, 54.4883f, 11017.484f), red_RedBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(red_RedBuff);

            //Red Side Krugs
            var redGolems = CreateJungleCamp(new Vector3(6317.0923f, 60.0f, 12146.458f), 11, TeamId.TEAM_PURPLE, "LesserCamp", 125.0f * 1000, spawnDuration: 0.86f);
            CreateJungleMonster("SRU_KrugMini11.1.1", "SRU_KrugMini", new Vector2(6317.0923f, 12146.458f), new Vector3(6365.6284f, 30.0f, 12226.984f), redGolems, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_Krug11.1.2", "SRU_Krug", new Vector2(6547.0923f, 12156.458f), new Vector3(6517.6284f, 30.0f, 12232.984f), redGolems, spawnAnimation: "spawn_NJ", aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(redGolems);

            //Baron
            var baron = CreateJungleCamp(new Vector3(5007.1235f, 60.0f, 10471.446f), 12, 0, "Baron", 1200.0f * 1000, revealEvent: 42, spawnDuration: 8.5f);
            CreateJungleMonster("SRU_Baron12.1.1", "SRU_Baron", new Vector2(5007.1235f, 10471.446f), new Vector3(4736.0586f, -71.0f, 10107.984f), baron, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi");
            CreateJungleMonster("SRU_BaronSpawn12.1.2", "SRU_BaronSpawn", new Vector2(5007.1235f, 10471.446f), new Vector3(4736.0586f, -71.0f, 10107.984f), baron, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(baron);

            //Blue Side Gromp
            var blueGreatGromp = CreateJungleCamp(new Vector3(2090.6284f, 60.0f, 8427.984f), 13, 0, "LesserCamp", 115.0f * 1000, spawnDuration: 3.2f);
            CreateJungleMonster("SRU_Gromp13.1.1", "SRU_Gromp", new Vector2(2090.6284f, 8427.984f), new Vector3(2338.0186f, 51.7773f, 8448.134f), blueGreatGromp, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(blueGreatGromp);

            //Red Side Gromp
            var redGreatGromp = CreateJungleCamp(new Vector3(12703.629f, 60.0f, 6443.984f), 14, 0, "LesserCamp", 115.0f * 1000, spawnDuration: 3.2f);
            CreateJungleMonster("SRU_Gromp14.1.1", "SRU_Gromp", new Vector2(12703.629f, 6443.984f), new Vector3(12323.828f, 55.5656f, 6272.774f), redGreatGromp, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(redGreatGromp);

            //Dragon pit Scuttle Crab
            var dragScuttle = CreateJungleCamp(new Vector3(10500.0f, 60.0f, 5170.0f), 15, 0, "LesserCamp", 150.0f * 1000, spawnDuration: 2.2f);
            CreateJungleMonster("Sru_Crab15.1.1", "Sru_Crab", new Vector2(10500.0f, 5170.0f), new Vector3(9830.0f, 0.0f, 5780.0f), dragScuttle, spawnAnimation: "crab_hide", aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(dragScuttle);

            //Baron pit Scuttle Crab
            var baronScuttle = CreateJungleCamp(new Vector3(4400.0f, 60.0f, 9600.0f), 16, 0, "LesserCamp", 150.0f * 1000, spawnDuration: 2.2f);
            CreateJungleMonster("Sru_Crab16.1.1", "Sru_Crab", new Vector2(4400.0f, 9600.0f), new Vector3(5240.0f, 0.0f, 8950.0f), baronScuttle, spawnAnimation: "crab_hide", aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(baronScuttle);
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

        public int GetAverageLevel()
        {
            float average = 1;
            var players = GetAllPlayers();
            foreach (var player in players)
            {
                average += player.Stats.Level / players.Count;
            }
            return (int)average;
        }

        public static void ForceCampSpawn()
        {
            forceSpawn = true;
        }

        public static float GetRespawnTimer(IMonsterCamp monsterCamp)
        {
            switch (monsterCamp.CampIndex)
            {
                case 1:
                case 4:
                case 7:
                case 10:
                    return 300.0f * 1000;
                case 12:
                    return 420.0f * 1000;
                case 6:
                    return 360.0f * 1000f;
                case 15:
                case 16:
                    return 180.0f * 1000f;
                default:
                    return 100.0f * 1000;
            }
        }
    }
}
