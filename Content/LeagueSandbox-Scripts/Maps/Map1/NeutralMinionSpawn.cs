using GameServerCore.Enums;
using GameServerLib.GameObjects;
using LeagueSandbox.GameServer.GameObjects;
using System.Collections.Generic;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;

namespace MapScripts.Map1
{
    public class NeutralMinionSpawn
    {
        private static bool forceSpawn;

        public static Dictionary<MonsterCamp, List<Monster>> MonsterCamps = new Dictionary<MonsterCamp, List<Monster>>();

        public static void InitializeCamps()
        {
            //Blue Side Blue Buff
            var blue_blueBuff = CreateJungleCamp(new Vector3(3632.7002f, 60.0f, 7600.373f), 1, TeamId.TEAM_BLUE, "Camp", 115.0f * 1000);
            MonsterCamps.Add(blue_blueBuff, new List<Monster>
            {
                CreateJungleMonster("AncientGolem1.1.1", "AncientGolem", new Vector2(3632.7002f, 7600.373f), new Vector3(3013.98f, 55.0703f, 7969.72f), blue_blueBuff, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("YoungLizard1.1.2", "YoungLizard", new Vector2(3552.7002f, 7799.373f), new Vector3(3013.98f, 55.0703f, 7969.72f), blue_blueBuff, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("YoungLizard1.1.3", "YoungLizard", new Vector2(3452.7002f, 7590.373f), new Vector3(3013.98f, 55.0703f, 7969.72f), blue_blueBuff, aiScript: "BasicJungleMonsterAI")
            });

            //Blue side Wolfs
            var blueWolves = CreateJungleCamp(new Vector3(3373.6782f, 60.0f, 6223.3457f), 2, TeamId.TEAM_BLUE, "LesserCamp", 125.0f * 1000);
            MonsterCamps.Add(blueWolves, new List<Monster> {
                CreateJungleMonster("GiantWolf2.1.1", "GiantWolf", new Vector2(3373.6782f, 6223.3457f), new Vector3(3294.0f, 46.0f, 6165.0f), blueWolves, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("Wolf2.1.2", "Wolf", new Vector2(3523.6782f, 6223.3457f), new Vector3(3294.0f, 46.0f, 6165.0f), blueWolves, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("Wolf2.1.3", "Wolf", new Vector2(3323.6782f, 6373.3457f), new Vector3(3294.0f, 46.0f, 6165.0f), blueWolves, aiScript: "BasicJungleMonsterAI")
            });

            //Blue Side Wraiths
            var blueWraiths = CreateJungleCamp(new Vector3(6300.05f, 60.0f, 5300.06f), 3, TeamId.TEAM_BLUE, "LesserCamp", 125.0f * 1000);
            MonsterCamps.Add(blueWraiths, new List<Monster>
            {
                CreateJungleMonster("Wraith3.1.1", "Wraith", new Vector2(6300.05f, 5300.06f), new Vector3(6552.0f, 48.0f, 5240.0f), blueWraiths, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("LesserWraith3.1.2", "LesserWraith", new Vector2(6523.0f, 5426.95f), new Vector3(6552.0f, 48.0f, 5240.0f), blueWraiths, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("LesserWraith3.1.3", "LesserWraith", new Vector2(6653.83f, 5278.29f), new Vector3(6552.0f, 48.0f, 5240.0f), blueWraiths, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("LesserWraith3.1.4", "LesserWraith", new Vector2(6582.915f, 5107.8857f), new Vector3(6552.0f, 48.0f, 5240.0f), blueWraiths, aiScript: "BasicJungleMonsterAI")
            });

            //Blue Side RedBuff
            var blue_RedBuff = CreateJungleCamp(new Vector3(7455.615f, 60.0f, 3890.2026f), 4, TeamId.TEAM_BLUE, "Camp", 115.0f * 1000);
            MonsterCamps.Add(blue_RedBuff, new List<Monster>
            {
                CreateJungleMonster("LizardElder4.1.1", "LizardElder", new Vector2(7455.615f, 3890.2026f), new Vector3(7348.0f, 48.0f, 3829.0f), blue_RedBuff, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("YoungLizard4.1.2", "YoungLizard", new Vector2(7460.615f, 3710.2026f), new Vector3(7348.0f, 48.0f, 3829.0f), blue_RedBuff, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("YoungLizard4.1.3", "YoungLizard", new Vector2(7237.615f, 3890.2026f), new Vector3(7348.0f, 48.0f, 3829.0f), blue_RedBuff, aiScript: "BasicJungleMonsterAI")
            });

            //Blue Side Golems
            var blueGolems = CreateJungleCamp(new Vector3(7916.8423f, 60.0f, 2533.9634f), 5, TeamId.TEAM_BLUE, "LesserCamp", 125.0f * 1000);
            MonsterCamps.Add(blueGolems, new List<Monster> {
                CreateJungleMonster("SmallGolem5.1.1", "SmallGolem", new Vector2(7916.8423f, 2533.9634f), new Vector3(7913.0f, 45.0f, 2421.0f), blueGolems, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("Golem5.1.2", "Golem", new Vector2(8216.842f, 2533.9634f), new Vector3(8163.0f, 45.0f, 2413.0f), blueGolems, aiScript: "BasicJungleMonsterAI")
            });

            //Dragon
            var dragon = CreateJungleCamp(new Vector3(9459.52f, 60.0f, 4193.03f), 6, TeamId.TEAM_UNKNOWN, "Dragon", 150.0f * 1000);
            MonsterCamps.Add(dragon, new List<Monster>
            {
                CreateJungleMonster("Dragon6.1.1", "Dragon", new Vector2(9459.52f, 4193.03f), new Vector3(9622.0f, -69.0f, 4490.0f), dragon, aiScript: "BasicJungleMonsterAI")
            });

            //Red Side BlueBuff
            var red_BlueBuff = CreateJungleCamp(new Vector3(10386.605f, 60.0f, 6811.1123f), 7, TeamId.TEAM_PURPLE, "Camp", 115.0f * 1000);
            MonsterCamps.Add(red_BlueBuff, new List<Monster>
            {
                CreateJungleMonster("AncientGolem7.1.1", "AncientGolem", new Vector2(10386.605f, 6811.1123f), new Vector3(11022.0f, 54.8568f, 6519.72f), red_BlueBuff, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("YoungLizard7.1.2", "YoungLizard", new Vector2(10586.605f, 6831.1123f), new Vector3(11022.0f, 54.8568f, 6519.72f), red_BlueBuff, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("YoungLizard4.1.3", "YoungLizard", new Vector2(10526.605f, 6601.1123f), new Vector3(11022.0f, 54.8568f, 6519.72f), red_BlueBuff, aiScript: "BasicJungleMonsterAI")
            });

            //Red side Wolfs
            var redWolves = CreateJungleCamp(new Vector3(10651.523f, 60.0f, 8116.4243f), 8, TeamId.TEAM_PURPLE, "LesserCamp", 125.0f * 1000);
            MonsterCamps.Add(redWolves, new List<Monster>
            {
                CreateJungleMonster("GiantWolf8.1.1", "GiantWolf", new Vector2(10651.523f, 8116.4243f), new Vector3(10721.0f, 53.0f, 8282.0f), redWolves, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("Wolf8.1.2", "Wolf", new Vector2(10651.523f, 7916.4243f), new Vector3(10721.0f, 53.0f, 8282.0f), redWolves, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("Wolf8.1.3", "Wolf", new Vector2(10451.523f, 8116.4243f), new Vector3(10721.0f, 53.0f, 8282.0f), redWolves, aiScript: "BasicJungleMonsterAI")
            });

            //Red Side Wraiths
            var redWraiths = CreateJungleCamp(new Vector3(7580.368f, 60.0f, 9250.405f), 9, TeamId.TEAM_PURPLE, "LesserCamp", 125.0f * 1000);
            MonsterCamps.Add(redWraiths, new List<Monster>
            {
                CreateJungleMonster("Wraith9.1.1", "Wraith", new Vector2(7580.368f, 9250.405f), new Vector3(7495.0f, 46.0f, 9259.0f), redWraiths, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("LesserWraith9.1.2", "LesserWraith", new Vector2(7480.368f, 9091.405f), new Vector3(7495.0f, 46.0f, 9259.0f), redWraiths, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("LesserWraith9.1.3", "LesserWraith", new Vector2(7350.368f, 9230.405f), new Vector3(7495.0f, 46.0f, 9259.0f), redWraiths, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("LesserWraith9.1.4", "LesserWraith", new Vector2(7450.368f, 9350.405f), new Vector3(7495.0f, 46.0f, 9259.0f), redWraiths, aiScript: "BasicJungleMonsterAI")
            });

            //Red Side RedBuff
            var red_RedBuff = CreateJungleCamp(new Vector3(6504.2407f, 60.0f, 10584.5625f), 10, TeamId.TEAM_PURPLE, "Camp", 115.0f * 1000);
            MonsterCamps.Add(red_RedBuff, new List<Monster>
            {
                CreateJungleMonster("LizardElder10.1.1", "LizardElder", new Vector2(6504.2407f, 10584.5625f), new Vector3(6618.0f, 45.0f, 10709.0f), red_RedBuff, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("YoungLizard10.1.2", "YoungLizard", new Vector2(6704.2407f, 10584.5625f), new Vector3(6618.0f, 45.0f, 10709.0f), red_RedBuff, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("YoungLizard10.1.3", "YoungLizard", new Vector2(6504.2407f, 10784.5625f), new Vector3(6618.0f, 45.0f, 10709.0f), red_RedBuff, aiScript: "BasicJungleMonsterAI")
            });

            //Red Side Golems
            var redGolems = CreateJungleCamp(new Vector3(5810.464f, 60.0f, 11925.474f), 11, TeamId.TEAM_PURPLE, "LesserCamp", 125.0f * 1000);
            MonsterCamps.Add(redGolems, new List<Monster>
            {
                CreateJungleMonster("SmallGolem11.1.1", "SmallGolem", new Vector2(5810.464f, 11925.474f), new Vector3(5859.0f, 30.0f, 12006.0f), redGolems, aiScript: "BasicJungleMonsterAI"),
                CreateJungleMonster("Golem11.1.2", "Golem", new Vector2(6140.464f, 11935.474f), new Vector3(6111.0f, 30.0f, 12012.0f), redGolems, aiScript: "BasicJungleMonsterAI")
            });

            //Baron
            var baron = CreateJungleCamp(new Vector3(4600.495f, 60.0f, 10250.462f), 12, TeamId.TEAM_UNKNOWN, "Baron", 900.0f * 1000);
            MonsterCamps.Add(baron, new List<Monster>
            {
                CreateJungleMonster("Worm12.1.1", "Worm", new Vector2(4600.495f, 10250.462f), new Vector3(4329.43f, -71.0f, 9887.0f), baron, aiScript: "BasicJungleMonsterAI")
            });

            //Blue Side GreatWraith (Old gromp)
            var blueGreatGromp = CreateJungleCamp(new Vector3(1684.0f, 60.0f, 8207.0f), 13, TeamId.TEAM_UNKNOWN, "LesserCamp", 125.0f * 1000);
            MonsterCamps.Add(blueGreatGromp, new List<Monster>
            {
                CreateJungleMonster("GreatWraith13.1.1", "GreatWraith", new Vector2(1684.0f, 8207.0f), new Vector3(2300.0f, 53.0f, 9720.0f), blueGreatGromp, aiScript: "BasicJungleMonsterAI")
            });

            //Red Side GreatWraith (Old gromp)
            var redGreatGromp = CreateJungleCamp(new Vector3(12337.0f, 60.0f, 6263.0f), 14, TeamId.TEAM_UNKNOWN, "LesserCamp", 125.0f * 1000);
            MonsterCamps.Add(redGreatGromp, new List<Monster>
            {
                CreateJungleMonster("GreatWraith14.1.1", "GreatWraith", new Vector2(12337.0f, 6263.0f), new Vector3(11826.0f, 52.0f, 4788.0f), redGreatGromp, aiScript: "BasicJungleMonsterAI")
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

        public static void SpawnCamp(MonsterCamp monsterCamp)
        {
            var averageLevel = GetPlayerAverageLevel();

            foreach (var monster in MonsterCamps[monsterCamp])
            {
                monster.UpdateInitialLevel(averageLevel);
                monster.Stats.Level = (byte)averageLevel;
                Monster campMonster = monsterCamp.AddMonster(monster);
                MonsterDataTable.UpdateStats(campMonster);
            }
        }

        public static void ForceCampSpawn()
        {
            forceSpawn = true;
        }

        public static float GetRespawnTimer(MonsterCamp monsterCamp)
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
                default:
                    return 50.0f * 1000;
            }
        }
    }
}
