using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Maps;
using LeagueSandbox.GameServer.Content;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static GameServerLib.API.APIMapFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;

namespace MapScripts.Map11
{
    public class CLASSIC : IMapScript
    {
        public virtual IMapScriptMetadata MapScriptMetadata { get; set; } = new MapScriptMetadata
        {
            MinionPathingOverride = true,
            EnableBuildingProtection = false
        };
        public IMapScriptHandler _map;
        public virtual IGlobalData GlobalData { get; set; } = new GlobalData();
        public bool HasFirstBloodHappened { get; set; } = false;
        public long NextSpawnTime { get; set; } = 90 * 1000;
        public string LaneMinionAI { get; set; } = "LaneMinionAI";
        public string LaneTurretAI { get; set; } = "TurretAI";

        public Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> PlayerSpawnPoints { get; }

        //Tower type enumeration might vary slightly from map to map, so we set that up here
        public TurretType GetTurretType(int trueIndex, LaneID lane, TeamId teamId)
        {
            TurretType returnType = TurretType.FOUNTAIN_TURRET;

            if (lane == LaneID.MIDDLE)
            {
                if (trueIndex < 3)
                {
                    returnType = TurretType.NEXUS_TURRET;
                    return returnType;
                }

                trueIndex -= 2;
            }

            switch (trueIndex)
            {
                case 1:
                case 4:
                case 5:
                    returnType = TurretType.INHIBITOR_TURRET;
                    break;
                case 2:
                    returnType = TurretType.INNER_TURRET;
                    break;
                case 3:
                    returnType = TurretType.OUTER_TURRET;
                    break;
            }

            return returnType;
        }

        //Nexus models
        //Nexus and Inhibitor model changes dont seem to take effect in-game, has to be investigated.
        public Dictionary<TeamId, string> NexusModels { get; set; }

        public Dictionary<TeamId, string> InhibitorModels { get; set; }
        public Dictionary<TeamId, Dictionary<TurretType, string>> TowerModels { get; set; }

        public Dictionary<TurretType, int[]> TurretItems { get; set; } = new Dictionary<TurretType, int[]>
        {
            { TurretType.OUTER_TURRET, new[] { 1500, 1501, 1502, 1503 } },
            { TurretType.INNER_TURRET, new[] { 1500, 1501, 1502, 1503, 1504 } },
            { TurretType.INHIBITOR_TURRET, new[] { 1501, 1502, 1503, 3178 } },
            { TurretType.NEXUS_TURRET, new[] { 1501, 1502, 1503, 3178 } }
        };

        //List of every path minions will take, separated by team and lane
        public Dictionary<LaneID, List<Vector2>> MinionPaths { get; set; } = new Dictionary<LaneID, List<Vector2>>
        {
                //Pathing coordinates for Top lane
                {LaneID.TOP, new List<Vector2> {
                    new Vector2(917.0f, 1725.0f),
                    new Vector2(1170.0f, 4041.0f),
                    new Vector2(861.0f, 6459.0f),
                    new Vector2(880.0f, 10180.0f),
                    new Vector2(1268.0f, 11675.0f),
                    new Vector2(2806.0f, 13075.0f),
                    new Vector2(3907.0f, 13243.0f),
                    new Vector2(7550.0f, 13407.0f),
                    new Vector2(10244.0f, 13238.0f),
                    new Vector2(10947.0f, 13135.0f),
                    new Vector2(12511.0f, 12776.0f) }
                },

                //Pathing coordinates for Mid lane
                {LaneID.MIDDLE, new List<Vector2> {
                    new Vector2(1418.0f, 1686.0f),
                    new Vector2(2997.0f, 2781.0f),
                    new Vector2(4472.0f, 4727.0f),
                    new Vector2(8375.0f, 8366.0f),
                    new Vector2(10948.0f, 10821.0f),
                    new Vector2(12511.0f, 12776.0f) }
                },

                //Pathing coordinates for Bot lane
                {LaneID.BOTTOM, new List<Vector2> {
                    new Vector2(1487.0f, 1302.0f),
                    new Vector2(3789.0f, 1346.0f),
                    new Vector2(6430.0f, 1005.0f),
                    new Vector2(10995.0f, 1234.0f),
                    new Vector2(12841.0f, 3051.0f),
                    new Vector2(13148.0f, 4202.0f),
                    new Vector2(13249.0f, 7884.0f),
                    new Vector2(12886.0f, 10356.0f),
                    new Vector2(12511.0f, 12776.0f) }
                }
        };


        //List of every wave type
        public Dictionary<string, List<MinionSpawnType>> MinionWaveTypes = new Dictionary<string, List<MinionSpawnType>>
        { {"RegularMinionWave", new List<MinionSpawnType>
        {
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER }
        },
        {"CannonMinionWave", new List<MinionSpawnType>{
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CANNON,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER }
        },
        {"SuperMinionWave", new List<MinionSpawnType>{
            MinionSpawnType.MINION_TYPE_SUPER,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER }
        },
        {"DoubleSuperMinionWave", new List<MinionSpawnType>{
            MinionSpawnType.MINION_TYPE_SUPER,
            MinionSpawnType.MINION_TYPE_SUPER,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER }
        }
        };

        //Here you setup the conditions of which wave will be spawned
        public Tuple<int, List<MinionSpawnType>> MinionWaveToSpawn(float gameTime, int cannonMinionCount, bool isInhibitorDead, bool areAllInhibitorsDead)
        {
            var cannonMinionTimestamps = new List<Tuple<long, int>>
            {
                new Tuple<long, int>(0, 2),
                new Tuple<long, int>(20 * 60 * 1000, 1),
                new Tuple<long, int>(35 * 60 * 1000, 0)
            };
            var cannonMinionCap = 2;

            foreach (var timestamp in cannonMinionTimestamps)
            {
                if (gameTime >= timestamp.Item1)
                {
                    cannonMinionCap = timestamp.Item2;
                }
            }
            var list = "RegularMinionWave";
            if (cannonMinionCount >= cannonMinionCap)
            {
                list = "CannonMinionWave";
            }

            if (isInhibitorDead)
            {
                list = "SuperMinionWave";
            }

            if (areAllInhibitorsDead)
            {
                list = "DoubleSuperMinionWave";
            }
            return new Tuple<int, List<MinionSpawnType>>(cannonMinionCap, MinionWaveTypes[list]);
        }

        //Minion models for this map
        public Dictionary<TeamId, Dictionary<MinionSpawnType, string>> MinionModels { get; set; } = new Dictionary<TeamId, Dictionary<MinionSpawnType, string>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<MinionSpawnType, string>{
                {MinionSpawnType.MINION_TYPE_MELEE, "SRU_OrderMinionMelee"},
                {MinionSpawnType.MINION_TYPE_CASTER, "SRU_OrderMinionRanged"},
                {MinionSpawnType.MINION_TYPE_CANNON, "SRU_OrderMinionSiege"},
                {MinionSpawnType.MINION_TYPE_SUPER, "SRU_OrderMinionSuper"}
            }},
            {TeamId.TEAM_PURPLE, new Dictionary<MinionSpawnType, string>{
                {MinionSpawnType.MINION_TYPE_MELEE, "SRU_ChaosMinionMelee"},
                {MinionSpawnType.MINION_TYPE_CASTER, "SRU_ChaosMinionRanged"},
                {MinionSpawnType.MINION_TYPE_CANNON, "SRU_ChaosMinionSiege"},
                {MinionSpawnType.MINION_TYPE_SUPER, "SRU_ChaosMinionSuper"}
            }}
        };

        //This function is executed in-between Loading the map structures and applying the structure protections. Is the first thing on this script to be executed
        public virtual void Init(IMapScriptHandler map)
        {
            _map = map;
            MapScriptMetadata.MinionSpawnEnabled = IsMinionSpawnEnabled();
            AddSurrender(1200000.0f, 300000.0f, 30.0f);

            //Blue Team Bot lane
            CreateTurret("Turret_T1_R_03_A", "SRUAP_Turret_Order1", new Vector2(10504.246f, 1029.7169f), TeamId.TEAM_BLUE, TurretType.OUTER_TURRET, LaneID.BOTTOM, LaneTurretAI);
            CreateTurret("Turret_T1_R_02_A", "SRUAP_Turret_Order2", new Vector2(6919.156f, 1483.5986f), TeamId.TEAM_BLUE, TurretType.INNER_TURRET, LaneID.BOTTOM, LaneTurretAI);
            CreateTurret("Turret_T1_C_07_A", "SRUAP_Turret_Order3", new Vector2(4281.712f, 1253.5687f), TeamId.TEAM_BLUE, TurretType.INHIBITOR_TURRET, LaneID.BOTTOM, LaneTurretAI);

            //Red Team Bot lane
            CreateTurret("Turret_T2_R_03_A", "SRUAP_Turret_Chaos1", new Vector2(13866.243f, 4505.2236f), TeamId.TEAM_PURPLE, TurretType.OUTER_TURRET, LaneID.BOTTOM, LaneTurretAI);
            CreateTurret("Turret_T2_R_02_A", "SRUAP_Turret_Chaos2", new Vector2(13327.417f, 8226.276f), TeamId.TEAM_PURPLE, TurretType.INNER_TURRET, LaneID.BOTTOM, LaneTurretAI);
            CreateTurret("Turret_T2_R_01_A", "SRUAP_Turret_Chaos3", new Vector2(13624.748f, 10572.771f), TeamId.TEAM_PURPLE, TurretType.INHIBITOR_TURRET, LaneID.BOTTOM, LaneTurretAI);

            //Blue Team Mid lane
            CreateTurret("Turret_T1_C_05_A", "SRUAP_Turret_Order1", new Vector2(5846.0967f, 6396.7505f), TeamId.TEAM_BLUE, TurretType.OUTER_TURRET, LaneID.MIDDLE, LaneTurretAI);
            CreateTurret("Turret_T1_C_04_A", "SRUAP_Turret_Order2", new Vector2(5048.0703f, 4812.8936f), TeamId.TEAM_BLUE, TurretType.INNER_TURRET, LaneID.MIDDLE, LaneTurretAI);
            CreateTurret("Turret_T1_C_03_A", "SRUAP_Turret_Order3", new Vector2(3651.9016f, 3696.424f), TeamId.TEAM_BLUE, TurretType.INHIBITOR_TURRET, LaneID.MIDDLE, LaneTurretAI);

            //Blue Team Nexus Towers
            CreateTurret("Turret_T1_C_01_A", "SRUAP_Turret_Order4", new Vector2(1748.2611f, 2270.7068f), TeamId.TEAM_BLUE, TurretType.NEXUS_TURRET, LaneID.MIDDLE, LaneTurretAI);
            CreateTurret("Turret_T1_C_02_A", "SRUAP_Turret_Order4", new Vector2(2177.64f, 1807.6298f), TeamId.TEAM_BLUE, TurretType.NEXUS_TURRET, LaneID.MIDDLE, LaneTurretAI);

            //Red Team Mid lane
            CreateTurret("Turret_T2_C_05_A", "SRUAP_Turret_Chaos1", new Vector2(8955.434f, 8510.48f), TeamId.TEAM_PURPLE, TurretType.OUTER_TURRET, LaneID.MIDDLE, LaneTurretAI);
            CreateTurret("Turret_T2_C_04_A", "SRUAP_Turret_Chaos2", new Vector2(9767.701f, 10113.608f), TeamId.TEAM_PURPLE, TurretType.INNER_TURRET, LaneID.MIDDLE, LaneTurretAI);
            CreateTurret("Turret_T2_C_03_A", "SRUAP_Turret_Chaos3", new Vector2(11134.814f, 11207.938f), TeamId.TEAM_PURPLE, TurretType.INHIBITOR_TURRET, LaneID.MIDDLE, LaneTurretAI);

            //Red Team Nexus Towers
            CreateTurret("Turret_T2_C_01_A", "SRUAP_Turret_Chaos4", new Vector2(13052.915f, 12612.381f), TeamId.TEAM_PURPLE, TurretType.NEXUS_TURRET, LaneID.MIDDLE, LaneTurretAI);
            CreateTurret("Turret_T2_C_02_A", "SRUAP_Turret_Chaos4", new Vector2(12611.182f, 13084.111f), TeamId.TEAM_PURPLE, TurretType.NEXUS_TURRET, LaneID.MIDDLE, LaneTurretAI);

            //Blue Team Fountain Tower
            CreateTurret("Turret_OrderTurretShrine_A", "SRUAP_Turret_Order5", new Vector2(105.92846f, 134.49403f), TeamId.TEAM_BLUE, TurretType.FOUNTAIN_TURRET, LaneID.NONE, LaneTurretAI);

            //Red Team Fountain Tower
            CreateTurret("Turret_ChaosTurretShrine_A", "SRUAP_Turret_Chaos5", new Vector2(14576.36f, 14693.827f), TeamId.TEAM_PURPLE, TurretType.FOUNTAIN_TURRET, LaneID.NONE, LaneTurretAI);

            //Blue Team Top Towers
            CreateTurret("Turret_T1_L_03_A", "SRUAP_Turret_Order1", new Vector2(981.28345f, 10441.454f), TeamId.TEAM_BLUE, TurretType.OUTER_TURRET, LaneID.TOP, LaneTurretAI);
            CreateTurret("Turret_T1_L_02_A", "SRUAP_Turret_Order2", new Vector2(1512.892f, 6699.57f), TeamId.TEAM_BLUE, TurretType.INNER_TURRET, LaneID.TOP, LaneTurretAI);
            CreateTurret("Turret_T1_C_06_A", "SRUAP_Turret_Order3", new Vector2(1169.9619f, 4287.4434f), TeamId.TEAM_BLUE, TurretType.INHIBITOR_TURRET, LaneID.TOP, LaneTurretAI);

            //Red Team Top Towers
            CreateTurret("Turret_T2_L_03_A", "SRUAP_Turret_Chaos1", new Vector2(4318.3037f, 13875.8f), TeamId.TEAM_PURPLE, TurretType.OUTER_TURRET, LaneID.TOP, LaneTurretAI);
            CreateTurret("Turret_T2_L_02_A", "SRUAP_Turret_Chaos2", new Vector2(7943.152f, 13411.799f), TeamId.TEAM_PURPLE, TurretType.INNER_TURRET, LaneID.TOP, LaneTurretAI);
            CreateTurret("Turret_T2_L_01_A", "SRUAP_Turret_Chaos3", new Vector2(10481.091f, 13650.535f), TeamId.TEAM_PURPLE, TurretType.INHIBITOR_TURRET, LaneID.TOP, LaneTurretAI);

            //Blue Team Inhibitors
            CreateInhibitor("Barracks_T1_L1", "SRUAP_OrderInhibitor", new Vector2(1171.8285f, 3571.784f), TeamId.TEAM_BLUE, LaneID.TOP, 214, 0);
            CreateInhibitor("Barracks_T1_C1", "SRUAP_OrderInhibitor", new Vector2(3203.0286f, 3208.784f), TeamId.TEAM_BLUE, LaneID.MIDDLE, 214, 0);
            CreateInhibitor("Barracks_T1_R1", "SRUAP_OrderInhibitor", new Vector2(3452.5286f, 1236.884f), TeamId.TEAM_BLUE, LaneID.BOTTOM, 214, 0);

            //Red Team Inhibitors
            CreateInhibitor("Barracks_T2_L1", "SRUAP_OrderInhibitor", new Vector2(11261.665f, 13676.563f), TeamId.TEAM_PURPLE, LaneID.TOP, 214, 0);
            CreateInhibitor("Barracks_T2_C1", "SRUAP_OrderInhibitor", new Vector2(11598.124f, 11667.8125f), TeamId.TEAM_PURPLE, LaneID.MIDDLE, 214, 0);
            CreateInhibitor("Barracks_T2_R1", "SRUAP_OrderInhibitor", new Vector2(13604.601f, 11316.011f), TeamId.TEAM_PURPLE, LaneID.BOTTOM, 214, 0);

            //Create Nexus
            CreateNexus("HQ_T1", "SRUAP_OrderNexus", new Vector2(1551.3535f, 1659.627f), TeamId.TEAM_BLUE, 353, 1700);
            CreateNexus("HQ_T2", "SRUAP_ChaosNexus", new Vector2(13142.73f, 12964.941f), TeamId.TEAM_PURPLE, 353, 1700);

            CreateFountain(TeamId.TEAM_BLUE, new Vector2(394.76892f, 461.57095f));
            CreateFountain(TeamId.TEAM_PURPLE, new Vector2(14340.419f, 14391.075f));

            CreateLevelProps.CreateProps();
        }

        IStatsModifier InitialTowerStatModifier = new StatsModifier();
        IStatsModifier InitialInhibTowerStatModifier = new StatsModifier();
        IStatsModifier InitialNexusTowerStatModifier = new StatsModifier();
        IStatsModifier TowerStatModifier = new StatsModifier();
        public virtual void OnMatchStart()
        {
            //The wiki says all towers get 200 health per enemy champion, but the health wouldn't match the replay's unless i used these values.
            InitialTowerStatModifier.HealthPoints.FlatBonus = 200.0f;
            InitialInhibTowerStatModifier.HealthPoints.FlatBonus = 190.0f;
            InitialNexusTowerStatModifier.HealthPoints.FlatBonus = 150.0f;

            InitialInhibTowerStatModifier.Armor.BaseBonus= 33.0f;
            InitialInhibTowerStatModifier.AttackDamage.BaseBonus= -20.0f;
            TowerStatModifier.AttackDamage.FlatBonus = 4.0f;

            foreach (var team in _map.TurretList.Keys)
            {
                foreach (var lane in _map.TurretList[team].Keys)
                {
                    foreach (var turret in _map.TurretList[team][lane])
                    {
                        AddUnitPerceptionBubble(turret, 800.0F, 25000.0F, team, true, collisionArea: 88.4F);

                        switch (turret.Type)
                        {
                            case TurretType.OUTER_TURRET:
                            case TurretType.INNER_TURRET:
                                turret.AddStatModifier(InitialTowerStatModifier);
                                break;
                            case TurretType.INHIBITOR_TURRET:
                                turret.AddStatModifier(InitialInhibTowerStatModifier);
                                break;
                            case TurretType.NEXUS_TURRET:
                                turret.AddStatModifier(InitialNexusTowerStatModifier);
                                break;
                        }

                        AddTurretItems(turret, GetTurretItems(turret.Type));

                        turret.Stats.CurrentHealth = turret.Stats.HealthPoints.Total;
                    }
                }
            }

            NeutralMinionSpawn.InitializeCamps();
            foreach (var nexus in _map.NexusList)
            {
                ApiEventManager.OnDeath.AddListener(this, nexus, OnNexusDeath, true);
            }
        }

        public void Update(float diff)
        {
            NeutralMinionSpawn.OnUpdate(diff);

            var gameTime = GameTime();

            if (!AllAnnouncementsAnnounced)
            {
                CheckInitialMapAnnouncements(gameTime);
            }

            if (gameTime >= timeCheck && timesApplied < 37)
            {
                UpdateTowerStats();
            }
        }

        float timeCheck = 30.0f * 1000;
        byte timesApplied = 0;
        public void UpdateTowerStats()
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

        public void AddTurretModifier(List<TurretType> turretTypes)
        {
            foreach (var team in _map.TurretList.Keys)
            {
                foreach (var lane in _map.TurretList[team].Keys)
                {
                    foreach (var turret in _map.TurretList[team][lane])
                    {
                        if (turretTypes.Contains(turret.Type))
                        {
                            turret.AddStatModifier(TowerStatModifier);
                        }
                    }
                }
            }
        }

        public void OnNexusDeath(IDeathData deathaData)
        {
            var nexus = deathaData.Unit;
            string particle = "SRU_Order_Nexus_Explosion";

            if (nexus.Team == TeamId.TEAM_PURPLE)
            {
                particle = "SRU_Chaos_Nexus_Explosion";
            }

            NotifySpawn(AddParticle(nexus, nexus, particle, nexus.Position, 10));
            EndGame(nexus.Team, new Vector3(nexus.Position.X, nexus.GetHeight(), nexus.Position.Y), deathData: deathaData);
        }

        public void SpawnAllCamps()
        {
            NeutralMinionSpawn.ForceCampSpawn();
        }

        bool AllAnnouncementsAnnounced = false;
        List<EventID> AnnouncedEvents = new List<EventID>();
        public void CheckInitialMapAnnouncements(float time)
        {
            if (time >= 90.0f * 1000)
            {
                // Minions have spawned
                NotifyMapAnnouncement(EventID.OnMinionsSpawn, 0);
                NotifyMapAnnouncement(EventID.OnNexusCrystalStart, 0);
                AllAnnouncementsAnnounced = true;
            }
            else if (time >= 60.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage2))
            {
                // 30 seconds until minions spawn
                NotifyMapAnnouncement(EventID.OnStartGameMessage2, _map.Id);
                AnnouncedEvents.Add(EventID.OnStartGameMessage2);
            }
            else if (time >= 30.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage1))
            {
                // Welcome to Summoners Rift
                NotifyMapAnnouncement(EventID.OnStartGameMessage1, _map.Id);
                AnnouncedEvents.Add(EventID.OnStartGameMessage1);
            }
        }
    }
}
