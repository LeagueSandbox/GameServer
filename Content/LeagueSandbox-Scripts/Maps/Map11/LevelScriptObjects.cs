using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using System.Collections.Generic;
using GameServerCore.Domain;
using System.Numerics;
using System.Linq;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects;
using            GameServerLib.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.Content;

namespace MapScripts.Map11
{
    public static class LevelScriptObjects
    {
        private static Dictionary<GameObjectTypes, List<MapObject>> _mapObjects;

        public static Dictionary<TeamId, Fountain> FountainList = new Dictionary<TeamId, Fountain>();
        public static Dictionary<string, MapObject> SpawnBarracks = new Dictionary<string, MapObject>();
        public static Dictionary<Lane, List<Vector2>> MinionPaths = new Dictionary<Lane, List<Vector2>> { { Lane.LANE_L, new List<Vector2>() }, { Lane.LANE_R, new List<Vector2>() } };
        public static Dictionary<TeamId, bool> AllInhibitorsAreDead = new Dictionary<TeamId, bool> { { TeamId.TEAM_BLUE, false }, { TeamId.TEAM_PURPLE, false } };
        static Dictionary<TeamId, Dictionary<Inhibitor, float>> DeadInhibitors = new Dictionary<TeamId, Dictionary<Inhibitor, float>> { { TeamId.TEAM_BLUE, new Dictionary<Inhibitor, float>() }, { TeamId.TEAM_PURPLE, new Dictionary<Inhibitor, float>() } };
        static List<Nexus> NexusList = new List<Nexus>();
        static string LaneTurretAI = "TurretAI";

        static Dictionary<TeamId, Dictionary<Lane, List<LaneTurret>>> TurretList = new Dictionary<TeamId, Dictionary<Lane, List<LaneTurret>>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<Lane, List<LaneTurret>>{
                { Lane.LANE_Unknown, new List<LaneTurret>()},
                { Lane.LANE_L, new List<LaneTurret>()},
                { Lane.LANE_C, new List<LaneTurret>()},
                { Lane.LANE_R, new List<LaneTurret>()}}
            },
            {TeamId.TEAM_PURPLE, new Dictionary<Lane, List<LaneTurret>>{
                { Lane.LANE_Unknown, new List<LaneTurret>()},
                { Lane.LANE_L, new List<LaneTurret>()},
                { Lane.LANE_C, new List<LaneTurret>()},
                { Lane.LANE_R, new List<LaneTurret>()}}
            }
        };

        public static Dictionary<TeamId, Dictionary<Lane, Inhibitor>> InhibitorList = new Dictionary<TeamId, Dictionary<Lane, Inhibitor>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<Lane, Inhibitor>{
                { Lane.LANE_L, null },
                { Lane.LANE_C, null },
                { Lane.LANE_R, null }}
            },
            {TeamId.TEAM_PURPLE, new Dictionary<Lane, Inhibitor>{
                { Lane.LANE_L, null },
                { Lane.LANE_C, null },
                { Lane.LANE_R, null }}
            }
        };

        //Turret Items
        public static Dictionary<TurretType, int[]> TurretItems { get; set; } = new Dictionary<TurretType, int[]>
        {
            { TurretType.OUTER_TURRET, new[] { 1500, 1501, 1502, 1503 } },
            { TurretType.INNER_TURRET, new[] { 1500, 1501, 1502, 1503, 1504 } },
            { TurretType.INHIBITOR_TURRET, new[] { 1501, 1502, 1503, 3178 } },
            { TurretType.NEXUS_TURRET, new[] { 1501, 1502, 1503, 3178 } }
        };

        public static void LoadObjects(Dictionary<GameObjectTypes, List<MapObject>> mapObjects)
        {
            _mapObjects = mapObjects;

            CreateBuildings();
            LoadProtection();
            LoadSpawnBarracks();
            LoadFountains();
        }

        static StatsModifier TowerStatModifier = new StatsModifier();
        public static void OnMatchStart()
        {
            LoadShops();
            StatsModifier InitialTowerHealthModifier = new StatsModifier();
            TowerStatModifier.AttackDamage.FlatBonus = 4.0f;

            Dictionary<TeamId, List<Champion>> Players = new Dictionary<TeamId, List<Champion>>
            {
                {TeamId.TEAM_BLUE, ApiFunctionManager.GetAllPlayersFromTeam(TeamId.TEAM_BLUE) },
                {TeamId.TEAM_PURPLE, ApiFunctionManager.GetAllPlayersFromTeam(TeamId.TEAM_PURPLE) }
            };

            foreach (var team in TurretList.Keys)
            {
                TeamId enemyTeam = TeamId.TEAM_BLUE;

                if (team == TeamId.TEAM_BLUE)
                {
                    enemyTeam = TeamId.TEAM_PURPLE;
                }

                foreach (var lane in TurretList[team].Keys)
                {
                    foreach (var turret in TurretList[team][lane])
                    {
                        AddUnitPerceptionBubble(turret, 800.0F, 25000.0F, team, true, collisionArea: 88.4F);

                        //The wiki says all towers get 200 health per enemy champion, but the health wouldn't match the replay's unless i used these values.
                        switch (turret.Type)
                        {
                            case TurretType.OUTER_TURRET:
                            case TurretType.INNER_TURRET:
                                InitialTowerHealthModifier.HealthPoints.BaseBonus = 200.0f * Players[enemyTeam].Count;
                                turret.AddStatModifier(InitialTowerHealthModifier);
                                break;
                            case TurretType.INHIBITOR_TURRET:
                                InitialTowerHealthModifier.HealthPoints.BaseBonus = 190.0f * Players[enemyTeam].Count;
                                InitialTowerHealthModifier.Armor.BaseBonus = 33.0f;
                                InitialTowerHealthModifier.AttackDamage.BaseBonus = -20.0f;
                                turret.AddStatModifier(InitialTowerHealthModifier);
                                break;
                            case TurretType.NEXUS_TURRET:
                                InitialTowerHealthModifier.HealthPoints.BaseBonus = 150.0f * Players[enemyTeam].Count;
                                turret.AddStatModifier(InitialTowerHealthModifier);
                                break;
                        }

                        AddTurretItems(turret, GetTurretItems(TurretItems, turret.Type));
                        turret.Stats.CurrentHealth = turret.Stats.HealthPoints.Total;
                    }
                }
            }
        }

        public static void OnUpdate(float diff)
        {
            var gameTime = GameTime();

            if (gameTime >= timeCheck && timesApplied < 37)
            {
                UpdateTowerStats();
            }

            foreach (var fountain in FountainList.Values)
            {
                fountain.Update(diff);
            }

            foreach (var team in DeadInhibitors.Keys)
            {
                foreach (var inhibitor in DeadInhibitors[team].Keys.ToList())
                {
                    DeadInhibitors[team][inhibitor] -= diff;
                    if (DeadInhibitors[team][inhibitor] <= 0)
                    {
                        inhibitor.Stats.CurrentHealth = inhibitor.Stats.HealthPoints.Total;
                        inhibitor.NotifyState();
                        DeadInhibitors[inhibitor.Team].Remove(inhibitor);
                    }
                    else if (DeadInhibitors[team][inhibitor] <= 15.0f * 1000)
                    {
                        inhibitor.SetState(DampenerState.RespawningState);
                    }
                    else if (!inhibitor.RespawnAnimationAnnounced && DeadInhibitors[team][inhibitor] <= 10.0f * 1000)
                    {
                        inhibitor.PlayAnimation("Respawn", -1, 0.0f, 1.0f, (AnimationFlags)9);
                        inhibitor.RespawnAnimationAnnounced = true;
                    }
                }
            }
        }

        public static void OnNexusDeath(DeathData deathaData)
        {
            var nexus = deathaData.Unit;
            string particle = "SRU_Order_Nexus_Explosion";

            if (nexus.Team == TeamId.TEAM_PURPLE)
            {
                //This particle effect doesn't seem to work
                particle = "SRU_Chaos_Nexus_Explosion";
            }

            NotifySpawnBroadcast(AddParticle(nexus, nexus, particle, nexus.Position, 10));
            EndGame(nexus.Team, new Vector3(nexus.Position.X, nexus.GetHeight(), nexus.Position.Y), deathData: deathaData);
        }

        public static void OnInhibitorDeath(DeathData deathData)
        {
            var inhibitor = deathData.Unit as Inhibitor;

            DeadInhibitors[inhibitor.Team].Add(inhibitor, inhibitor.RespawnTime * 1000);
            inhibitor.RespawnAnimationAnnounced = false;

            if (DeadInhibitors[inhibitor.Team].Count == InhibitorList[inhibitor.Team].Count)
            {
                AllInhibitorsAreDead[inhibitor.Team] = true;
            }
        }

        static float timeCheck = 10.0f * 1000;
        static byte timesApplied = 0;
        public static void UpdateTowerStats()
        {
            if (timesApplied < 7)
            {
                if (timesApplied == 6)
                {
                    timeCheck += 30.0f * 1000;
                }
                AddTurretModifier(new List<TurretType> { TurretType.OUTER_TURRET });
            }
            else if (timesApplied < 27)
            {
                AddTurretModifier(new List<TurretType> { TurretType.INNER_TURRET, TurretType.INHIBITOR_TURRET, TurretType.NEXUS_TURRET });
            }
            else
            {
                AddTurretModifier(new List<TurretType> { TurretType.INHIBITOR_TURRET, TurretType.NEXUS_TURRET });
            }


            timesApplied++;
            timeCheck += 60 * 1000;
        }

        public static void AddTurretModifier(List<TurretType> turretTypes)
        {
            foreach (var team in TurretList.Keys)
            {
                foreach (var lane in TurretList[team].Keys)
                {
                    foreach (var turret in TurretList[team][lane])
                    {
                        if (turretTypes.Contains(turret.Type))
                        {
                            turret.AddStatModifier(TowerStatModifier);
                        }
                    }
                }
            }
        }

        static void LoadFountains()
        {
            FountainList.Add(TeamId.TEAM_BLUE, CreateFountain(TeamId.TEAM_BLUE, new Vector2(394.76892f, 461.57095f)));
            FountainList.Add(TeamId.TEAM_PURPLE, CreateFountain(TeamId.TEAM_PURPLE, new Vector2(14340.419f, 14391.075f)));
        }

        static void LoadShops()
        {
            foreach (var shop in _mapObjects[GameObjectTypes.ObjBuilding_Shop])
            {
                CreateShop(shop.Name, new Vector2(shop.CentralPoint.X, shop.CentralPoint.Z), shop.GetTeamID());
            }
        }

        static void LoadSpawnBarracks()
        {
            SpawnBarracks.Add("__P_Chaos_Spawn_Barracks__R01", CreateLaneMinionSpawnPos("__P_Chaos_Spawn_Barracks__R01", new Vector3(13719.46f, 119.2171f, 12845.7f)));
            SpawnBarracks.Add("__P_Chaos_Spawn_Barracks__C01", CreateLaneMinionSpawnPos("__P_Chaos_Spawn_Barracks__C01", new Vector3(12776.26f, 110.7853f, 12784.75f)));
            SpawnBarracks.Add("__P_Chaos_Spawn_Barracks__L01", CreateLaneMinionSpawnPos("__P_Chaos_Spawn_Barracks__L01", new Vector3(12800.61f, 128.4584f, 13745.36f)));
            SpawnBarracks.Add("__P_Order_Spawn_Barracks__R01", CreateLaneMinionSpawnPos("__P_Order_Spawn_Barracks__R01", new Vector3(2034.091f, 128.7497f, 1171.083f)));
            SpawnBarracks.Add("__P_Order_Spawn_Barracks__C01", CreateLaneMinionSpawnPos("__P_Order_Spawn_Barracks__C01", new Vector3(2008.126f, 119.2706f, 2079.599f)));
            SpawnBarracks.Add("__P_Order_Spawn_Barracks__L01", CreateLaneMinionSpawnPos("__P_Order_Spawn_Barracks__L01", new Vector3(1109.416f, 124.5788f, 2091.567f)));
        }

        static void CreateBuildings()
        {
            //Blue Team Bot lane
            var blueBotOuterTurret = CreateLaneTurret("Turret_T1_R_03_A", "SRUAP_Turret_Order1", new Vector2(10504.246f, 1029.7169f), TeamId.TEAM_BLUE, TurretType.OUTER_TURRET, Lane.LANE_R, LaneTurretAI);
            var blueBotInnerTurret = CreateLaneTurret("Turret_T1_R_02_A", "SRUAP_Turret_Order2", new Vector2(6919.156f, 1483.5986f), TeamId.TEAM_BLUE, TurretType.INNER_TURRET, Lane.LANE_R, LaneTurretAI);
            var blueBotInhibtorTurret = CreateLaneTurret("Turret_T1_C_07_A", "SRUAP_Turret_Order3", new Vector2(4281.712f, 1253.5687f), TeamId.TEAM_BLUE, TurretType.INHIBITOR_TURRET, Lane.LANE_R, LaneTurretAI);
            TurretList[TeamId.TEAM_BLUE][Lane.LANE_R].AddRange(new List<LaneTurret> { { blueBotOuterTurret }, { blueBotInnerTurret }, { blueBotInhibtorTurret } });

            //Red Team Bot lane
            var redBotOuterTurret = CreateLaneTurret("Turret_T2_R_03_A", "SRUAP_Turret_Chaos1", new Vector2(13866.243f, 4505.2236f), TeamId.TEAM_PURPLE, TurretType.OUTER_TURRET, Lane.LANE_R, LaneTurretAI);
            var redBotInnerTurret = CreateLaneTurret("Turret_T2_R_02_A", "SRUAP_Turret_Chaos2", new Vector2(13327.417f, 8226.276f), TeamId.TEAM_PURPLE, TurretType.INNER_TURRET, Lane.LANE_R, LaneTurretAI);
            var redBotInhibtorTurret = CreateLaneTurret("Turret_T2_R_01_A", "SRUAP_Turret_Chaos3", new Vector2(13624.748f, 10572.771f), TeamId.TEAM_PURPLE, TurretType.INHIBITOR_TURRET, Lane.LANE_R, LaneTurretAI);
            TurretList[TeamId.TEAM_PURPLE][Lane.LANE_R].AddRange(new List<LaneTurret> { { redBotOuterTurret }, { redBotInnerTurret }, { redBotInhibtorTurret } });

            //Blue Team Mid lane
            var blueMidOuterTurret = CreateLaneTurret("Turret_T1_C_05_A", "SRUAP_Turret_Order1", new Vector2(5846.0967f, 6396.7505f), TeamId.TEAM_BLUE, TurretType.OUTER_TURRET, Lane.LANE_C, LaneTurretAI);
            var blueMidInnerTurret = CreateLaneTurret("Turret_T1_C_04_A", "SRUAP_Turret_Order2", new Vector2(5048.0703f, 4812.8936f), TeamId.TEAM_BLUE, TurretType.INNER_TURRET, Lane.LANE_C, LaneTurretAI);
            var blueMidInhibtorTurret = CreateLaneTurret("Turret_T1_C_03_A", "SRUAP_Turret_Order3", new Vector2(3651.9016f, 3696.424f), TeamId.TEAM_BLUE, TurretType.INHIBITOR_TURRET, Lane.LANE_C, LaneTurretAI);
            TurretList[TeamId.TEAM_BLUE][Lane.LANE_C].AddRange(new List<LaneTurret> { { blueMidOuterTurret }, { blueMidInnerTurret }, { blueMidInhibtorTurret } });

            //Blue Team Nexus Towers
            var blueTopNexusTurrets = CreateLaneTurret("Turret_T1_C_01_A", "SRUAP_Turret_Order4", new Vector2(1748.2611f, 2270.7068f), TeamId.TEAM_BLUE, TurretType.NEXUS_TURRET, Lane.LANE_C, LaneTurretAI);
            var blueBotNexusTurrets = CreateLaneTurret("Turret_T1_C_02_A", "SRUAP_Turret_Order4", new Vector2(2177.64f, 1807.6298f), TeamId.TEAM_BLUE, TurretType.NEXUS_TURRET, Lane.LANE_C, LaneTurretAI);
            TurretList[TeamId.TEAM_BLUE][Lane.LANE_C].AddRange(new List<LaneTurret> { { blueTopNexusTurrets }, { blueBotNexusTurrets } });

            //Red Team Mid lane
            var redMidOuterTurret = CreateLaneTurret("Turret_T2_C_05_A", "SRUAP_Turret_Chaos1", new Vector2(8955.434f, 8510.48f), TeamId.TEAM_PURPLE, TurretType.OUTER_TURRET, Lane.LANE_C, LaneTurretAI);
            var redMidInnerTurret = CreateLaneTurret("Turret_T2_C_04_A", "SRUAP_Turret_Chaos2", new Vector2(9767.701f, 10113.608f), TeamId.TEAM_PURPLE, TurretType.INNER_TURRET, Lane.LANE_C, LaneTurretAI);
            var redMidInhibtorTurret = CreateLaneTurret("Turret_T2_C_03_A", "SRUAP_Turret_Chaos3", new Vector2(11134.814f, 11207.938f), TeamId.TEAM_PURPLE, TurretType.INHIBITOR_TURRET, Lane.LANE_C, LaneTurretAI);
            TurretList[TeamId.TEAM_PURPLE][Lane.LANE_C].AddRange(new List<LaneTurret> { { redMidOuterTurret }, { redMidInnerTurret }, { redMidInhibtorTurret } });

            //Red Team Nexus Towers
            var redTopNexusTurrets = CreateLaneTurret("Turret_T2_C_01_A", "SRUAP_Turret_Chaos4", new Vector2(13052.915f, 12612.381f), TeamId.TEAM_PURPLE, TurretType.NEXUS_TURRET, Lane.LANE_C, LaneTurretAI);
            var redBotNexusTurrets = CreateLaneTurret("Turret_T2_C_02_A", "SRUAP_Turret_Chaos4", new Vector2(12611.182f, 13084.111f), TeamId.TEAM_PURPLE, TurretType.NEXUS_TURRET, Lane.LANE_C, LaneTurretAI);
            TurretList[TeamId.TEAM_PURPLE][Lane.LANE_C].AddRange(new List<LaneTurret> { { redTopNexusTurrets }, { redBotNexusTurrets } });

            //Blue Team Fountain Tower
            TurretList[TeamId.TEAM_BLUE][Lane.LANE_Unknown].Add(CreateLaneTurret("Turret_OrderTurretShrine_A", "SRUAP_Turret_Order5", new Vector2(105.92846f, 134.49403f), TeamId.TEAM_BLUE, TurretType.FOUNTAIN_TURRET, Lane.LANE_Unknown, LaneTurretAI));

            //Red Team Fountain Tower
            TurretList[TeamId.TEAM_PURPLE][Lane.LANE_Unknown].Add(CreateLaneTurret("Turret_ChaosTurretShrine_A", "SRUAP_Turret_Chaos5", new Vector2(14576.36f, 14693.827f), TeamId.TEAM_PURPLE, TurretType.FOUNTAIN_TURRET, Lane.LANE_Unknown, LaneTurretAI));

            //Blue Team Top Towers
            var blueTopOuterTurret = CreateLaneTurret("Turret_T1_L_03_A", "SRUAP_Turret_Order1", new Vector2(981.28345f, 10441.454f), TeamId.TEAM_BLUE, TurretType.OUTER_TURRET, Lane.LANE_L, LaneTurretAI);
            var blueTopInnerTurret = CreateLaneTurret("Turret_T1_L_02_A", "SRUAP_Turret_Order2", new Vector2(1512.892f, 6699.57f), TeamId.TEAM_BLUE, TurretType.INNER_TURRET, Lane.LANE_L, LaneTurretAI);
            var blueTopInhibtorTurret = CreateLaneTurret("Turret_T1_C_06_A", "SRUAP_Turret_Order3", new Vector2(1169.9619f, 4287.4434f), TeamId.TEAM_BLUE, TurretType.INHIBITOR_TURRET, Lane.LANE_L, LaneTurretAI);
            TurretList[TeamId.TEAM_BLUE][Lane.LANE_L].AddRange(new List<LaneTurret> { { blueTopOuterTurret }, { blueTopInnerTurret }, { blueTopInhibtorTurret } });

            //Red Team Top Towers
            var redTopOuterTurret = CreateLaneTurret("Turret_T2_L_03_A", "SRUAP_Turret_Chaos1", new Vector2(4318.3037f, 13875.8f), TeamId.TEAM_PURPLE, TurretType.OUTER_TURRET, Lane.LANE_L, LaneTurretAI);
            var redTopInnerTurret = CreateLaneTurret("Turret_T2_L_02_A", "SRUAP_Turret_Chaos2", new Vector2(7943.152f, 13411.799f), TeamId.TEAM_PURPLE, TurretType.INNER_TURRET, Lane.LANE_L, LaneTurretAI);
            var redTopInhibtorTurret = CreateLaneTurret("Turret_T2_L_01_A", "SRUAP_Turret_Chaos3", new Vector2(10481.091f, 13650.535f), TeamId.TEAM_PURPLE, TurretType.INHIBITOR_TURRET, Lane.LANE_L, LaneTurretAI);
            TurretList[TeamId.TEAM_PURPLE][Lane.LANE_L].AddRange(new List<LaneTurret> { { redTopOuterTurret }, { redTopInnerTurret }, { redTopInhibtorTurret } });



            //Blue Team Inhibitors
            CreateInhib("Barracks_T1_L1", "SRUAP_OrderInhibitor", new Vector2(1171.8285f, 3571.784f), TeamId.TEAM_BLUE, Lane.LANE_L);
            CreateInhib("Barracks_T1_C1", "SRUAP_OrderInhibitor", new Vector2(3203.0286f, 3208.784f), TeamId.TEAM_BLUE, Lane.LANE_C);
            CreateInhib("Barracks_T1_R1", "SRUAP_OrderInhibitor", new Vector2(3452.5286f, 1236.884f), TeamId.TEAM_BLUE, Lane.LANE_R);


            //Red Team Inhibitors
            CreateInhib("Barracks_T2_L1", "SRUAP_ChaosInhibitor", new Vector2(11261.665f, 13676.563f), TeamId.TEAM_PURPLE, Lane.LANE_L);
            CreateInhib("Barracks_T2_C1", "SRUAP_ChaosInhibitor", new Vector2(11598.124f, 11667.8125f), TeamId.TEAM_PURPLE, Lane.LANE_C);
            CreateInhib("Barracks_T2_R1", "SRUAP_ChaosInhibitor", new Vector2(13604.601f, 11316.011f), TeamId.TEAM_PURPLE, Lane.LANE_R);

            //Create Nexus
            CreateNex("HQ_T1", "SRUAP_OrderNexus", new Vector2(1551.3535f, 1659.627f), TeamId.TEAM_BLUE);
            CreateNex("HQ_T2", "SRUAP_ChaosNexus", new Vector2(13142.73f, 12964.941f), TeamId.TEAM_PURPLE);

            foreach (var team in InhibitorList.Keys)
            {
                foreach (var inhibitor in InhibitorList[team].Values)
                {
                    ApiEventManager.OnDeath.AddListener(inhibitor, inhibitor, OnInhibitorDeath, false);
                    inhibitor.RespawnTime = 300.0f;
                }
            }
            foreach (var nexus in NexusList)
            {
                ApiEventManager.OnDeath.AddListener(nexus, nexus, OnNexusDeath, true);
            }

            SpawnBuildings();
        }

        static void SpawnBuildings()
        {
            foreach (var nexus in NexusList)
            {
                AddObject(nexus);
            }
            foreach (var team in InhibitorList.Keys)
            {
                foreach (var lane in InhibitorList[team].Keys)
                {
                    AddObject(InhibitorList[team][lane]);

                    //Spawn Turrets
                    foreach (var turret in TurretList[team][lane])
                    {
                        // Adds Turrets
                        AddObject(turret);
                    }
                }

                //Spawn FountainTurrets
                foreach (var turret in TurretList[team][Lane.LANE_Unknown])
                {
                    // Adds FountainTurret
                    AddObject(turret);
                }
            }
        }

        public static void CreateInhib(string name, string model, Vector2 position, TeamId team, Lane laneID)
        {
            var inhibitorStats = new Stats();
            inhibitorStats.HealthPoints.BaseValue = GlobalData.BarrackVariables.MaxHP;
            inhibitorStats.Armor.BaseValue = GlobalData.BarrackVariables.Armor;
            inhibitorStats.CurrentHealth = inhibitorStats.HealthPoints.BaseValue;
            InhibitorList[team][laneID] = CreateInhibitor(name, model, position, team, laneID, 214, 0, inhibitorStats);
        }

        public static void CreateNex(string name, string model, Vector2 position, TeamId team)
        {
            var nexusStats = new Stats();
            nexusStats.HealthPoints.BaseValue = 5500.0f;
            nexusStats.CurrentHealth = nexusStats.HealthPoints.BaseValue;

            NexusList.Add(CreateNexus(name, model, position, team, 353, 1700, nexusStats));
        }

        static void LoadProtection()
        {
            //I can't help but feel there's a better way to do this
            Dictionary<TeamId, List<Inhibitor>> TeamInhibitors = new Dictionary<TeamId, List<Inhibitor>> { { TeamId.TEAM_BLUE, new List<Inhibitor>() }, { TeamId.TEAM_PURPLE, new List<Inhibitor>() } };
            foreach (var teams in InhibitorList.Keys)
            {
                foreach (var lane in InhibitorList[teams].Keys)
                {
                    TeamInhibitors[teams].Add(InhibitorList[teams][lane]);
                }
            }

            foreach (var nexus in NexusList)
            {
                // Adds Protection to Nexus
                AddProtection(nexus, TurretList[nexus.Team][Lane.LANE_C].FindAll(turret => turret.Type == TurretType.NEXUS_TURRET).ToArray(), TeamInhibitors[nexus.Team].ToArray());
            }

            foreach (var InhibTeam in TeamInhibitors.Keys)
            {
                foreach (var inhibitor in TeamInhibitors[InhibTeam])
                {
                    var inhibitorTurret = TurretList[inhibitor.Team][inhibitor.Lane].First(turret => turret.Type == TurretType.INHIBITOR_TURRET);

                    // Adds Protection to Inhibitors
                    if (inhibitorTurret != null)
                    {
                        // Depends on the first available inhibitor turret.
                        AddProtection(inhibitor, false, inhibitorTurret);
                    }

                    // Adds Protection to Turrets
                    foreach (var turret in TurretList[inhibitor.Team][inhibitor.Lane])
                    {
                        if (turret.Type == TurretType.NEXUS_TURRET)
                        {
                            AddProtection(turret, false, TeamInhibitors[inhibitor.Team].ToArray());
                        }
                        else if (turret.Type == TurretType.INHIBITOR_TURRET)
                        {
                            AddProtection(turret, false, TurretList[inhibitor.Team][inhibitor.Lane].First(dependTurret => dependTurret.Type == TurretType.INNER_TURRET));
                        }
                        else if (turret.Type == TurretType.INNER_TURRET)
                        {
                            //Checks if there are outer turrets
                            if (TurretList[inhibitor.Team][inhibitor.Lane].Any(outerTurret => outerTurret.Type == TurretType.OUTER_TURRET))
                            {
                                AddProtection(turret, false, TurretList[inhibitor.Team][inhibitor.Lane].First(dependTurret => dependTurret.Type == TurretType.OUTER_TURRET));
                            }
                        }
                    }
                }
            }
        }
    }
}
