using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using System.Collections.Generic;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;

namespace MapScripts.Map11
{
    public class NeutralMinionSpawn
    {
        private static bool forceSpawn;

        public static Dictionary<IMonsterCamp, List<IMonster>> MonsterCamps = new Dictionary<IMonsterCamp, List<IMonster>>();

        public static void InitializeCamps()
        {
            //Blue Side Blue Buff
            var blue_blueBuff = CreateJungleCamp(new Vector3(3871.48846f, 60.0f, 7901.05403f), 1, TeamId.TEAM_BLUE, "Camp", 113.9f * 1000, spawnDuration: 1.1f);
            MonsterCamps.Add(blue_blueBuff, new List<IMonster> 
            {
                CreateJungleMonster("SRU_Blue1.1.1", "SRU_Blue", new Vector2(3871.48846f, 7901.05403f), new Vector3(3420.60846f, 55.0703f, 8190.70403f), blue_blueBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_BlueMini1.1.2", "SRU_BlueMini", new Vector2(3777.69846f, 8103.47403f), new Vector3(3420.60846f, 55.0703f, 8190.70403f), blue_blueBuff, spawnAnimation: "Spawn", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_BlueMini21.1.3", "SRU_BlueMini2", new Vector2(3627.91846f, 7838.85403f), new Vector3(3420.60846f, 55.0703f, 8190.70403f), blue_blueBuff, spawnAnimation: "Spawn", aiScript: "BasicJungleMonsterAi")
            });

            //Blue side Wolfs
            var blueWolves = CreateJungleCamp(new Vector3(3780.6284f, 60.0f, 6443.984f), 2, TeamId.TEAM_BLUE, "LesserCamp", 114.34f * 1000, spawnDuration: 0.66f);
            MonsterCamps.Add(blueWolves, new List<IMonster>
            {
                CreateJungleMonster("SRU_Murkwolf2.1.1", "SRU_Murkwolf", new Vector2(3780.6284f, 6443.984f), new Vector3(3700.62846f, 46.0f, 6385.98403f), blueWolves, spawnAnimation: "Spawn", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_MurkwolfMini2.1.2", "SRU_MurkwolfMini", new Vector2(3980.62846f, 6443.98403f), new Vector3(3758.91846f, 55.6167f, 6225.84403f), blueWolves, spawnAnimation: "spawn_mini1", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_MurkwolfMini2.1.3", "SRU_MurkwolfMini", new Vector2(3730.62846f, 6593.98403f), new Vector3(3540.36846f, 55.6125f, 6472.31403f), blueWolves, spawnAnimation: "spawn_mini2", aiScript: "BasicJungleMonsterAi")
            });

            //Blue Side Wraiths
            var blueWraiths = CreateJungleCamp(new Vector3(6706.67846f, 60.0f, 5521.04403f), 3, TeamId.TEAM_BLUE, "LesserCamp", 114.0f * 1000, spawnDuration: 1.0f);
            MonsterCamps.Add(blueWraiths, new List<IMonster>
            {
                CreateJungleMonster("SRU_Razorbeak3.1.1", "SRU_Razorbeak", new Vector2(6706.67846f, 5521.04403f), new Vector3(6958.62846f, 48.0f, 5460.98403f), blueWraiths, spawnAnimation: "Spawn", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_RazorbeakMini3.1.2", "SRU_RazorbeakMini", new Vector2(6929.62846f, 5647.93403f), new Vector3(6949.25846f, 56.8276f, 5549.06403f), blueWraiths, spawnAnimation: "Spawn4", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_RazorbeakMini3.1.3", "SRU_RazorbeakMini", new Vector2(7060.45846f, 5499.27403f), new Vector3(6958.62846f, 48.0f, 5460.98403f), blueWraiths, spawnAnimation: "Spawn2", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_RazorbeakMini3.1.4", "SRU_RazorbeakMini", new Vector2(6914.71846f, 5325.35403f), new Vector3(6989.543255f, 50.27832f, 5328.869772f),blueWraiths, spawnAnimation: "Spawn3", aiScript: "BasicJungleMonsterAi")
            });

            //Blue Side RedBuff
            var blue_RedBuff = CreateJungleCamp(new Vector3(7862.243694f, 60.0f, 4111.186667f), 4, TeamId.TEAM_BLUE, "Camp", 113.34f * 1000, spawnDuration: 1.66f);
            MonsterCamps.Add(blue_RedBuff, new List<IMonster>
            {
                CreateJungleMonster("SRU_Red4.1.1", "SRU_Red", new Vector2(7862.243694f, 4111.186667f), new Vector3(7622.38846f, 54.426f, 3947.19403f), blue_RedBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_RedMini4.1.2", "SRU_RedMini", new Vector2(7574.12846f, 4161.37403f), new Vector3(7636.56846f, 57.4629f, 3896.93403f), blue_RedBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_RedMini4.1.3", "SRU_RedMini", new Vector2(7918.50846f, 3880.83403f), new Vector3(7636.56846f, 57.4629f, 3896.93403f), blue_RedBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi")
            });
            
            //Blue Side Golems
            var blueGolems = CreateJungleCamp(new Vector3(8323.470745f, 60.0f, 2754.947409f), 5, TeamId.TEAM_BLUE, "LesserCamp", 114.14f * 1000, spawnDuration: 0.86f);
            MonsterCamps.Add(blueGolems, new List<IMonster>
            {
                CreateJungleMonster("SRU_KrugMini5.1.1", "SRU_KrugMini", new Vector2(8323.471f, 2754.9475f), new Vector3(8319.62846f, 45.0f, 2641.98403f), blueGolems, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_Krug5.1.2", "SRU_Krug", new Vector2(8532.470745f, 2737.947409f), new Vector3(8569.62846f, 45.0f, 2633.98403f), blueGolems, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi")
            });

            //Dragon
            var dragon = CreateJungleCamp(new Vector3(9866.14846f, 60.0f, 4414.01403f), 6, 0, "Dragon", 143.5f * 1000, revealEvent: 43, spawnDuration: 6.5f);
            MonsterCamps.Add(dragon, new List<IMonster>
            {
                CreateJungleMonster("Dragon6.1.1", "SRU_Dragon", new Vector2(9866.14846f, 4414.01403f), new Vector3(10517.62846f, -67.0f, 5171.98403f), dragon, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi")
            });

            //Red Side BlueBuff
            var red_BlueBuff = CreateJungleCamp(new Vector3(10931.72846f, 60.0f, 6990.84403f), 7, TeamId.TEAM_PURPLE, "Camp", 113.9f * 1000, spawnDuration: 1.1f);
            MonsterCamps.Add(red_BlueBuff, new List<IMonster>
            {
                CreateJungleMonster("SRU_Blue7.1.1", "SRU_Blue", new Vector2(10931.72846f, 6990.84403f), new Vector3(11428.62846f, 54.8568f, 6740.70403f), red_BlueBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_BlueMini7.1.2", "SRU_BlueMini", new Vector2(11210.32846f, 7064.44403f), new Vector3(11428.62846f, 54.8568f, 6740.70403f), red_BlueBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_BlueMini27.1.3", "SRU_BlueMini2", new Vector2(11097.62846f, 6789.68403f), new Vector3(11428.62846f, 54.8568f, 6740.70403f), red_BlueBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi")
            });

            //Red side Wolfs
            var redWolves = CreateJungleCamp(new Vector3(11008.151898f, 60.0f, 8387.408346f), 8, TeamId.TEAM_PURPLE, "LesserCamp", 114.34f * 1000, spawnDuration: 0.66f);
            MonsterCamps.Add(redWolves, new List<IMonster>
            {
                CreateJungleMonster("SRU_Murkwolf8.1.1", "SRU_Murkwolf", new Vector2(11008.151898f, 8387.408346f), new Vector3(11127.62846f, 53.0f, 8502.98403f), redWolves, spawnAnimation: "Spawn", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_MurkwolfMini8.1.2", "SRU_MurkwolfMini", new Vector2(11058.151898f, 8217.408346f), new Vector3(11285.82846f, 60.8542f, 8381.71403f), redWolves, spawnAnimation: "spawn_mini2", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_MurkwolfMini8.1.3", "SRU_MurkwolfMini", new Vector2(10808.151898f, 8387.408346f), new Vector3(11024.72846f, 64.4202f, 8585.54403f), redWolves, spawnAnimation: "spawn_mini1", aiScript: "BasicJungleMonsterAi")
            });

            //Red Side Wraiths
            var redWraiths = CreateJungleCamp(new Vector3(7986.996624f, 60.0f, 9471.389059f), 9, TeamId.TEAM_PURPLE, "LesserCamp", 114.33f * 1000, spawnDuration: 0.67f);
            MonsterCamps.Add(redWraiths, new List<IMonster>
            {
                CreateJungleMonster("SRU_Razorbeak9.1.1", "SRU_Razorbeak", new Vector2(7986.996624f, 9471.389059f), new Vector3(7901.62846f, 46.0f, 9479.98403f), redWraiths, spawnAnimation: "North_Spawn", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_RazorbeakMini9.1.2", "SRU_RazorbeakMini", new Vector2(7886.996624f, 9312.390059f), new Vector3(7901.62846f, 46.0f, 9479.98403f), redWraiths, spawnAnimation: "North_Spawn4", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_RazorbeakMini9.1.3", "SRU_RazorbeakMini", new Vector2(7756.996624f, 9451.389059f), new Vector3(7795.69846f, 52.3425f, 9491.01403f), redWraiths, spawnAnimation: "North_Spawn2", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_RazorbeakMini9.1.4", "SRU_RazorbeakMini", new Vector2(7854.38846f, 9610.47403f), new Vector3(7849.22846f, 52.1437f, 9644.08403f), redWraiths, spawnAnimation: "North_Spawn3", aiScript: "BasicJungleMonsterAi")
            });

            //Red Side RedBuff
            var red_RedBuff = CreateJungleCamp(new Vector3(7016.869183f, 60.0f, 10775.54653f), 10, TeamId.TEAM_PURPLE, "Camp", 113.34f * 1000, spawnDuration: 1.66f);
            MonsterCamps.Add(red_RedBuff, new List<IMonster>
            {
                CreateJungleMonster("SRU_Red10.1.1", "SRU_Red", new Vector2(7016.869f, 10775.547f), new Vector3(7257.22846f, 56.4084f, 11071.28403f), red_RedBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_RedMini10.1.2", "SRU_RedMini", new Vector2(7294.63846f, 10795.78403f), new Vector3(7113.10846f, 54.4883f, 11017.48403f), red_RedBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_RedMini10.1.3", "SRU_RedMini", new Vector2(6907.08846f, 11041.68403f), new Vector3(7113.10846f, 54.4883f, 11017.48403f), red_RedBuff, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi")
            });

            //Red Side Krugs
            var redGolems = CreateJungleCamp(new Vector3(6317.092327f, 60.0f, 12146.457663f), 11, TeamId.TEAM_PURPLE, "LesserCamp", 114.14f * 1000, spawnDuration: 0.86f);
            MonsterCamps.Add(redGolems, new List<IMonster>
            {
                CreateJungleMonster("SRU_KrugMini11.1.1", "SRU_KrugMini", new Vector2(6317.092327f, 12146.457663f), new Vector3(6365.62846f, 30.0f, 12226.98403f), redGolems, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_Krug11.1.2", "SRU_Krug", new Vector2(6547.092327f, 12156.457663f), new Vector3(6517.62846f, 30.0f, 12232.98403f), redGolems, spawnAnimation: "spawn_NJ", aiScript: "BasicJungleMonsterAi")
            });

            //Baron
            var baron = CreateJungleCamp(new Vector3(5007.123577f, 60.0f, 10471.445944f), 12, 0, "Baron", 1191.5f * 1000, revealEvent: 42, spawnDuration: 8.5f);
            MonsterCamps.Add(baron, new List<IMonster>
            {
                CreateJungleMonster("SRU_Baron12.1.1", "SRU_Baron", new Vector2(5007.123577f, 10471.445944f), new Vector3(4736.05846f, -71.0f, 10107.98403f), baron, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi"),
                CreateJungleMonster("SRU_BaronSpawn12.1.2", "SRU_BaronSpawn", new Vector2(5007.123577f, 10471.445944f), new Vector3(4736.05846f, -71.0f, 10107.98403f), baron, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi")
            });

            //Blue Side Gromp
            var blueGreatGromp = CreateJungleCamp(new Vector3(2090.62846f, 60.0f, 8427.98403f), 13, 0, "LesserCamp", 111.8f * 1000, spawnDuration: 3.2f);
            MonsterCamps.Add(blueGreatGromp, new List<IMonster>
            {
                CreateJungleMonster("SRU_Gromp13.1.1", "SRU_Gromp", new Vector2(2090.62846f, 8427.98403f), new Vector3(2338.01846f, 51.7773f, 8448.13403f), blueGreatGromp, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi")
            });

            //Red Side Gromp
            var redGreatGromp = CreateJungleCamp(new Vector3(12703.62846f, 60.0f, 6443.98403f), 14, 0, "LesserCamp", 111.8f * 1000, spawnDuration: 3.2f);
            MonsterCamps.Add(redGreatGromp, new List<IMonster>
            {
                CreateJungleMonster("SRU_Gromp14.1.1", "SRU_Gromp", new Vector2(12703.62846f, 6443.98403f), new Vector3(12323.82846f, 55.5656f, 6272.77403f), redGreatGromp, spawnAnimation: "spawn", aiScript: "BasicJungleMonsterAi")
            });

            //Dragon pit Scuttle Crab
            var dragScuttle = CreateJungleCamp(new Vector3(10500.0f, 60.0f, 5170.0f), 15, 0, "LesserCamp", 147.8f * 1000, spawnDuration: 2.2f);
            MonsterCamps.Add(dragScuttle, new List<IMonster>
            {
                CreateJungleMonster("Sru_Crab15.1.1", "Sru_Crab", new Vector2(10500.0f, 5170.0f), new Vector3(9830.0f, 0.0f, 5780.0f), dragScuttle, spawnAnimation: "crab_hide")
            });

            //Baron pit Scuttle Crab
            var baronScuttle = CreateJungleCamp(new Vector3(4400.0f, 60.0f, 9600.0f), 16, 0, "LesserCamp", 147.8f * 1000, spawnDuration: 2.2f);
            MonsterCamps.Add(baronScuttle, new List<IMonster>
            {
                CreateJungleMonster("Sru_Crab16.1.1", "Sru_Crab", new Vector2(4400.0f, 9600.0f), new Vector3(5240.0f, 0.0f, 8950.0f), baronScuttle, spawnAnimation: "crab_hide")
            });
        }

        public static void OnUpdate(float diff)
        {
            foreach (var camp in MonsterCamps.Keys)
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

        public static void SpawnCamp(IMonsterCamp monsterCamp)
        {
            var averageLevel = GetPlayerAverageLevel();

            foreach (var monster in MonsterCamps[monsterCamp])
            {
                monster.UpdateInitialLevel(averageLevel);
                monster.Stats.Level = (byte)averageLevel;
                IMonster campMonster = monsterCamp.AddMonster(monster);
                MonsterDataTable.UpdateStats(campMonster);
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
