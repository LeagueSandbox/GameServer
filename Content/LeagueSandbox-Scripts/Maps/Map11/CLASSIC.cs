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

namespace MapScripts.Map11
{
    public class CLASSIC : IMapScript
    {
        public virtual IMapScriptMetadata MapScriptMetadata { get; set; } = new MapScriptMetadata
        {
            MinionPathingOverride = true,
            EnableBuildingProtection = false
        };
        private bool forceSpawn;
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
        public Dictionary<TeamId, string> NexusModels { get; set; } = new Dictionary<TeamId, string>
        {
            {TeamId.TEAM_BLUE, "SRUAP_OrderNexus" },
            {TeamId.TEAM_PURPLE, "SRUAP_ChaosNexus" }
        };
        //Inhib models
        public Dictionary<TeamId, string> InhibitorModels { get; set; } = new Dictionary<TeamId, string>
        {
            {TeamId.TEAM_BLUE, "SRUAP_OrderInhibitor" },
            {TeamId.TEAM_PURPLE, "SRUAP_ChaosInhibitor" }
        };
        //Tower Models
        public Dictionary<TeamId, Dictionary<TurretType, string>> TowerModels { get; set; } = new Dictionary<TeamId, Dictionary<TurretType, string>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "OrderTurretShrine" },
                {TurretType.NEXUS_TURRET, "OrderTurretAngel" },
                {TurretType.INHIBITOR_TURRET, "OrderTurretDragon" },
                {TurretType.INNER_TURRET, "OrderTurretNormal2" },
                {TurretType.OUTER_TURRET, "OrderTurretNormal" },
            } },
            {TeamId.TEAM_PURPLE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "ChaosTurretShrine" },
                {TurretType.NEXUS_TURRET, "ChaosTurretNormal" },
                {TurretType.INHIBITOR_TURRET, "ChaosTurretGiant" },
                {TurretType.INNER_TURRET, "ChaosTurretWorm2" },
                {TurretType.OUTER_TURRET, "ChaosTurretWorm" },
            } }
        };

        //Turret Items
        public Dictionary<TurretType, int[]> TurretItems { get; set; } = new Dictionary<TurretType, int[]>
        {
            { TurretType.OUTER_TURRET, new[] { 1500, 1501, 1502, 1503 } },
            { TurretType.INNER_TURRET, new[] { 1500, 1501, 1502, 1503, 1504 } },
            { TurretType.INHIBITOR_TURRET, new[] { 1501, 1502, 1503, 1505 } },
            { TurretType.NEXUS_TURRET, new[] { 1501, 1502, 1503, 1505 } }
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
                {MinionSpawnType.MINION_TYPE_MELEE, "Blue_Minion_Basic"},
                {MinionSpawnType.MINION_TYPE_CASTER, "Blue_Minion_Wizard"},
                {MinionSpawnType.MINION_TYPE_CANNON, "Blue_Minion_MechCannon"},
                {MinionSpawnType.MINION_TYPE_SUPER, "Blue_Minion_MechMelee"}
            }},
            {TeamId.TEAM_PURPLE, new Dictionary<MinionSpawnType, string>{
                {MinionSpawnType.MINION_TYPE_MELEE, "Red_Minion_Basic"},
                {MinionSpawnType.MINION_TYPE_CASTER, "Red_Minion_Wizard"},
                {MinionSpawnType.MINION_TYPE_CANNON, "Red_Minion_MechCannon"},
                {MinionSpawnType.MINION_TYPE_SUPER, "Red_Minion_MechMelee"}
            }}
        };

        //This function is executed in-between Loading the map structures and applying the structure protections. Is the first thing on this script to be executed
        public virtual void Init(IMapScriptHandler map)
        {
            _map = map;
            MapScriptMetadata.MinionSpawnEnabled = map.IsMinionSpawnEnabled();
            map.AddSurrender(1200000.0f, 300000.0f, 30.0f);

            //Blue Team Bot lane
            _map.CreateTower("Turret_T1_R_03_A", "SRUAP_Turret_Order1", new Vector2(10504.246f, 1029.7169f), TeamId.TEAM_BLUE, TurretType.OUTER_TURRET, LaneID.BOTTOM, LaneTurretAI);
            _map.CreateTower("Turret_T1_R_02_A", "SRUAP_Turret_Order2", new Vector2(6919.156f, 1483.5986f), TeamId.TEAM_BLUE, TurretType.INNER_TURRET, LaneID.BOTTOM, LaneTurretAI);
            _map.CreateTower("Turret_T1_C_07_A", "SRUAP_Turret_Order3", new Vector2(4281.712f, 1253.5687f), TeamId.TEAM_BLUE, TurretType.INHIBITOR_TURRET, LaneID.BOTTOM, LaneTurretAI);

            //Red Team Bot lane
            _map.CreateTower("Turret_T2_R_03_A", "SRUAP_Turret_Chaos1", new Vector2(13866.243f, 4505.2236f), TeamId.TEAM_PURPLE, TurretType.OUTER_TURRET, LaneID.BOTTOM, LaneTurretAI);
            _map.CreateTower("Turret_T2_R_02_A", "SRUAP_Turret_Chaos2", new Vector2(13327.417f, 8226.276f), TeamId.TEAM_PURPLE, TurretType.INNER_TURRET, LaneID.BOTTOM, LaneTurretAI);
            _map.CreateTower("Turret_T2_R_01_A", "SRUAP_Turret_Chaos3", new Vector2(13624.748f, 10572.771f), TeamId.TEAM_PURPLE, TurretType.INHIBITOR_TURRET, LaneID.BOTTOM, LaneTurretAI);

            //Blue Team Mid lane
            _map.CreateTower("Turret_T1_C_05_A", "SRUAP_Turret_Order1", new Vector2(5846.0967f, 6396.7505f), TeamId.TEAM_BLUE, TurretType.OUTER_TURRET, LaneID.MIDDLE, LaneTurretAI);
            _map.CreateTower("Turret_T1_C_04_A", "SRUAP_Turret_Order2", new Vector2(5048.0703f, 4812.8936f), TeamId.TEAM_BLUE, TurretType.INNER_TURRET, LaneID.MIDDLE, LaneTurretAI);
            _map.CreateTower("Turret_T1_C_03_A", "SRUAP_Turret_Order3", new Vector2(3651.9016f, 3696.424f), TeamId.TEAM_BLUE, TurretType.INHIBITOR_TURRET, LaneID.MIDDLE, LaneTurretAI);

            //Blue Team Nexus Towers
            _map.CreateTower("Turret_T1_C_01_A", "SRUAP_Turret_Order4", new Vector2(1748.2611f, 2270.7068f), TeamId.TEAM_BLUE, TurretType.NEXUS_TURRET, LaneID.MIDDLE, LaneTurretAI);
            _map.CreateTower("Turret_T1_C_02_A", "SRUAP_Turret_Order4", new Vector2(2177.64f, 1807.6298f), TeamId.TEAM_BLUE, TurretType.NEXUS_TURRET, LaneID.MIDDLE, LaneTurretAI);

            //Red Team Mid lane
            _map.CreateTower("Turret_T2_C_05_A", "SRUAP_Turret_Chaos1", new Vector2(8955.434f, 8510.48f), TeamId.TEAM_PURPLE, TurretType.OUTER_TURRET, LaneID.MIDDLE, LaneTurretAI);
            _map.CreateTower("Turret_T2_C_04_A", "SRUAP_Turret_Chaos2", new Vector2(9767.701f, 10113.608f), TeamId.TEAM_PURPLE, TurretType.INNER_TURRET, LaneID.MIDDLE, LaneTurretAI);
            _map.CreateTower("Turret_T2_C_03_A", "SRUAP_Turret_Chaos3", new Vector2(11134.814f, 11207.938f), TeamId.TEAM_PURPLE, TurretType.INHIBITOR_TURRET, LaneID.MIDDLE, LaneTurretAI);

            //Red Team Nexus Towers
            _map.CreateTower("Turret_T2_C_01_A", "SRUAP_Turret_Chaos4", new Vector2(13052.915f, 12612.381f), TeamId.TEAM_PURPLE, TurretType.NEXUS_TURRET, LaneID.MIDDLE, LaneTurretAI);
            _map.CreateTower("Turret_T2_C_02_A", "SRUAP_Turret_Chaos4", new Vector2(12611.182f, 13084.111f), TeamId.TEAM_PURPLE, TurretType.NEXUS_TURRET, LaneID.MIDDLE, LaneTurretAI);

            //Blue Team Fountain Tower
            _map.CreateTower("Turret_OrderTurretShrine_A", "SRUAP_Turret_Order5", new Vector2(105.92846f, 134.49403f), TeamId.TEAM_BLUE, TurretType.FOUNTAIN_TURRET, LaneID.NONE, LaneTurretAI);

            //Red Team Fountain Tower
            _map.CreateTower("Turret_ChaosTurretShrine_A", "SRUAP_Turret_Chaos5", new Vector2(14576.36f, 14693.827f), TeamId.TEAM_PURPLE, TurretType.FOUNTAIN_TURRET, LaneID.NONE, LaneTurretAI);

            //Blue Team Top Towers
            _map.CreateTower("Turret_T1_L_03_A", "SRUAP_Turret_Order1", new Vector2(981.28345f, 10441.454f), TeamId.TEAM_BLUE, TurretType.OUTER_TURRET, LaneID.TOP, LaneTurretAI);
            _map.CreateTower("Turret_T1_L_02_A", "SRUAP_Turret_Order2", new Vector2(1512.892f, 6699.57f), TeamId.TEAM_BLUE, TurretType.INNER_TURRET, LaneID.TOP, LaneTurretAI);
            _map.CreateTower("Turret_T1_C_06_A", "SRUAP_Turret_Order3", new Vector2(1169.9619f, 4287.4434f), TeamId.TEAM_BLUE, TurretType.INHIBITOR_TURRET, LaneID.TOP, LaneTurretAI);

            //Red Team Top Towers
            _map.CreateTower("Turret_T2_L_03_A", "SRUAP_Turret_Chaos1", new Vector2(4318.3037f, 13875.8f), TeamId.TEAM_PURPLE, TurretType.OUTER_TURRET, LaneID.TOP, LaneTurretAI);
            _map.CreateTower("Turret_T2_L_02_A", "SRUAP_Turret_Chaos2", new Vector2(7943.152f, 13411.799f), TeamId.TEAM_PURPLE, TurretType.INNER_TURRET, LaneID.TOP, LaneTurretAI);
            _map.CreateTower("Turret_T2_L_01_A", "SRUAP_Turret_Chaos3", new Vector2(10481.091f, 13650.535f), TeamId.TEAM_PURPLE, TurretType.INHIBITOR_TURRET, LaneID.TOP, LaneTurretAI);

            //Blue Team Inhibitors
            _map.CreateInhibitor("Barracks_T1_L1", "SRUAP_OrderInhibitor", new Vector2(1171.8285f, 3571.784f), TeamId.TEAM_BLUE, LaneID.TOP, 214, 0);
            _map.CreateInhibitor("Barracks_T1_C1", "SRUAP_OrderInhibitor", new Vector2(3203.0286f, 3208.784f), TeamId.TEAM_BLUE, LaneID.MIDDLE, 214, 0);
            _map.CreateInhibitor("Barracks_T1_R1", "SRUAP_OrderInhibitor", new Vector2(3452.5286f, 1236.884f), TeamId.TEAM_BLUE, LaneID.BOTTOM, 214, 0);

            //Red Team Inhibitors
            _map.CreateInhibitor("Barracks_T2_L1", "SRUAP_OrderInhibitor", new Vector2(11261.665f, 13676.563f), TeamId.TEAM_PURPLE, LaneID.TOP, 214, 0);
            _map.CreateInhibitor("Barracks_T2_C1", "SRUAP_OrderInhibitor", new Vector2(11598.124f, 11667.8125f), TeamId.TEAM_PURPLE, LaneID.MIDDLE, 214, 0);
            _map.CreateInhibitor("Barracks_T2_R1", "SRUAP_OrderInhibitor", new Vector2(13604.601f, 11316.011f), TeamId.TEAM_PURPLE, LaneID.BOTTOM, 214, 0);

            //Create Nexus
            _map.CreateNexus("HQ_T1", "SRUAP_OrderNexus", new Vector2(3452.5286f, 1236.884f), TeamId.TEAM_BLUE, 353, 1700);
            _map.CreateNexus("HQ_T2", "SRUAP_ChaosNexus", new Vector2(13142.73f, 12964.941f), TeamId.TEAM_PURPLE, 353, 1700);
            SetupMapProps();
        }
        List<IMonsterCamp> MonsterCamps = new List<IMonsterCamp>();
        public virtual void OnMatchStart()
        {
            foreach (var team in _map.TurretList.Keys)
            {
                foreach (var lane in _map.TurretList[team].Keys)
                {
                    foreach (var turret in _map.TurretList[team][lane])
                    {
                        AddUnitPerceptionBubble(turret, 800.0F, 25000.0F, team, true, collisionArea: 88.4F);
                    }
                }
            }
            /*foreach (var nexus in _map.NexusList)
            {
                ApiEventManager.OnDeath.AddListener(this, nexus, OnNexusDeath, true);
            }
            SetupJungleCamps();*/
        }

        public void Update(float diff)
        {
            var gameTime = _map.GameTime();
            if (gameTime >= 120 * 1000)
            {
                MapScriptMetadata.IsKillGoldRewardReductionActive = false;
            }

            foreach (var camp in MonsterCamps)
            {
                if (!camp.IsAlive)
                {
                    camp.RespawnTimer -= diff;
                    if (camp.RespawnTimer <= 0 || forceSpawn)
                    {
                        _map.SpawnCamp(camp);
                        camp.RespawnTimer = GetRespawnTimer(camp);
                    }
                }
            }

            if (!AllAnnouncementsAnnounced)
            {
                CheckInitialMapAnnouncements(gameTime);
            }

            if (forceSpawn)
            {
                forceSpawn = false;
            }
        }

        public float GetRespawnTimer(IMonsterCamp monsterCamp)
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

        public void OnNexusDeath(IDeathData deathaData)
        {
            var nexus = deathaData.Unit;
            _map.EndGame(nexus.Team, new Vector3(nexus.Position.X, nexus.GetHeight(), nexus.Position.Y), deathData: deathaData);
        }

        public void SpawnAllCamps()
        {
            forceSpawn = true;
        }

        public float GetGoldFor(IAttackableUnit u)
        {
            if (!(u is ILaneMinion m))
            {
                if (!(u is IChampion c))
                {
                    return 0.0f;
                }

                var gold = 300.0f; //normal gold for a kill
                if (c.KillDeathCounter < 5 && c.KillDeathCounter >= 0)
                {
                    if (c.KillDeathCounter == 0)
                    {
                        return gold;
                    }

                    for (var i = c.KillDeathCounter; i > 1; --i)
                    {
                        gold += gold * 0.165f;
                    }

                    return gold;
                }

                if (c.KillDeathCounter >= 5)
                {
                    return 500.0f;
                }

                if (c.KillDeathCounter >= 0)
                    return 0.0f;

                var firstDeathGold = gold - gold * 0.085f;

                if (c.KillDeathCounter == -1)
                {
                    return firstDeathGold;
                }

                for (var i = c.KillDeathCounter; i < -1; ++i)
                {
                    firstDeathGold -= firstDeathGold * 0.2f;
                }

                if (firstDeathGold < 50)
                {
                    firstDeathGold = 50;
                }

                return firstDeathGold;
            }

            var dic = new Dictionary<MinionSpawnType, float>
            {
                { MinionSpawnType.MINION_TYPE_MELEE, 19.8f + 0.2f * (int)(_map.GameTime() / (90 * 1000)) },
                { MinionSpawnType.MINION_TYPE_CASTER, 16.8f + 0.2f * (int)(_map.GameTime() / (90 * 1000)) },
                { MinionSpawnType.MINION_TYPE_CANNON, 40.0f + 0.5f * (int)(_map.GameTime() / (90 * 1000)) },
                { MinionSpawnType.MINION_TYPE_SUPER, 40.0f + 1.0f * (int)(_map.GameTime() / (180 * 1000)) }
            };

            if (!dic.ContainsKey(m.MinionSpawnType))
            {
                return 0.0f;
            }

            return dic[m.MinionSpawnType];
        }

        public float GetExperienceFor(IAttackableUnit u)
        {
            if (!(u is ILaneMinion m))
            {
                return 0.0f;
            }

            var dic = new Dictionary<MinionSpawnType, float>
            {
                { MinionSpawnType.MINION_TYPE_MELEE, 64.0f },
                { MinionSpawnType.MINION_TYPE_CASTER, 32.0f },
                { MinionSpawnType.MINION_TYPE_CANNON, 92.0f },
                { MinionSpawnType.MINION_TYPE_SUPER, 97.0f }
            };

            if (!dic.ContainsKey(m.MinionSpawnType))
            {
                return 0.0f;
            }

            return dic[m.MinionSpawnType];
        }

        public void SetMinionStats(ILaneMinion m)
        {
            // Same for all minions
            m.Stats.MoveSpeed.BaseValue = 325.0f;

            switch (m.MinionSpawnType)
            {
                case MinionSpawnType.MINION_TYPE_MELEE:
                    m.Stats.CurrentHealth = 475.0f + 20.0f * (int)(_map.GameTime() / (180 * 1000));
                    m.Stats.HealthPoints.BaseValue = 475.0f + 20.0f * (int)(_map.GameTime() / (180 * 1000));
                    m.Stats.AttackDamage.BaseValue = 12.0f + 1.0f * (int)(_map.GameTime() / (180 * 1000));
                    m.Stats.Range.BaseValue = 180.0f;
                    m.Stats.AttackSpeedFlat = 1.250f;
                    m.IsMelee = true;
                    break;
                case MinionSpawnType.MINION_TYPE_CASTER:
                    m.Stats.CurrentHealth = 279.0f + 7.5f * (int)(_map.GameTime() / (90 * 1000));
                    m.Stats.HealthPoints.BaseValue = 279.0f + 7.5f * (int)(_map.GameTime() / (90 * 1000));
                    m.Stats.AttackDamage.BaseValue = 23.0f + 1.0f * (int)(_map.GameTime() / (90 * 1000));
                    m.Stats.Range.BaseValue = 600.0f;
                    m.Stats.AttackSpeedFlat = 0.670f;
                    break;
                case MinionSpawnType.MINION_TYPE_CANNON:
                    m.Stats.CurrentHealth = 700.0f + 27.0f * (int)(_map.GameTime() / (180 * 1000));
                    m.Stats.HealthPoints.BaseValue = 700.0f + 27.0f * (int)(_map.GameTime() / (180 * 1000));
                    m.Stats.AttackDamage.BaseValue = 40.0f + 3.0f * (int)(_map.GameTime() / (180 * 1000));
                    m.Stats.Range.BaseValue = 450.0f;
                    m.Stats.AttackSpeedFlat = 1.0f;
                    break;
                case MinionSpawnType.MINION_TYPE_SUPER:
                    m.Stats.CurrentHealth = 1500.0f + 200.0f * (int)(_map.GameTime() / (180 * 1000));
                    m.Stats.HealthPoints.BaseValue = 1500.0f + 200.0f * (int)(_map.GameTime() / (180 * 1000));
                    m.Stats.AttackDamage.BaseValue = 190.0f + 10.0f * (int)(_map.GameTime() / (180 * 1000));
                    m.Stats.Range.BaseValue = 170.0f;
                    m.Stats.AttackSpeedFlat = 0.694f;
                    m.Stats.Armor.BaseValue = 30.0f;
                    m.Stats.MagicResist.BaseValue = -30.0f;
                    m.IsMelee = true;
                    break;
            }
        }

        bool AllAnnouncementsAnnounced = false;
        List<EventID> AnnouncedEvents = new List<EventID>();
        public void CheckInitialMapAnnouncements(float time)
        {
            if (time >= 90.0f * 1000)
            {
                // Minions have spawned
                _map.NotifyMapAnnouncement(EventID.OnMinionsSpawn, 0);
                _map.NotifyMapAnnouncement(EventID.OnNexusCrystalStart, 0);
                AllAnnouncementsAnnounced = true;
            }
            else if (time >= 60.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage2))
            {
                // 30 seconds until minions spawn
                _map.NotifyMapAnnouncement(EventID.OnStartGameMessage2, _map.Id);
                AnnouncedEvents.Add(EventID.OnStartGameMessage2);
            }
            else if (time >= 30.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage1))
            {
                // Welcome to Summoners Rift
                _map.NotifyMapAnnouncement(EventID.OnStartGameMessage1, _map.Id);
                AnnouncedEvents.Add(EventID.OnStartGameMessage1);
            }
        }

        public void SetupJungleCamps()
        {
            //Blue Side Blue Buff
            var blue_blueBuff = _map.CreateJungleCamp(new Vector3(3632.7002f, 60.0f, 7600.373f), 1, TeamId.TEAM_BLUE, "Camp", 115.0f * 1000);
            _map.CreateJungleMonster("AncientGolem1.1.1", "AncientGolem", new Vector2(3632.7002f, 7600.373f), new Vector3(3013.98f, 55.0703f, 7969.72f), blue_blueBuff, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("YoungLizard1.1.2", "YoungLizard", new Vector2(3552.7002f, 7799.373f), new Vector3(3013.98f, 55.0703f, 7969.72f), blue_blueBuff, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("YoungLizard1.1.3", "YoungLizard", new Vector2(3452.7002f, 7590.373f), new Vector3(3013.98f, 55.0703f, 7969.72f), blue_blueBuff, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(blue_blueBuff);

            //Blue side Wolfs
            var blueWolves = _map.CreateJungleCamp(new Vector3(3373.6782f, 60.0f, 6223.3457f), 2, TeamId.TEAM_BLUE, "LesserCamp", 125.0f * 1000);
            _map.CreateJungleMonster("GiantWolf2.1.1", "GiantWolf", new Vector2(3373.6782f, 6223.3457f), new Vector3(3294.0f, 46.0f, 6165.0f), blueWolves, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("Wolf2.1.2", "Wolf", new Vector2(3523.6782f, 6223.3457f), new Vector3(3294.0f, 46.0f, 6165.0f), blueWolves, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("Wolf2.1.3", "Wolf", new Vector2(3323.6782f, 6373.3457f), new Vector3(3294.0f, 46.0f, 6165.0f), blueWolves, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(blueWolves);


            //Blue Side Wraiths
            var blueWraiths = _map.CreateJungleCamp(new Vector3(6300.05f, 60.0f, 5300.06f), 3, TeamId.TEAM_BLUE, "LesserCamp", 125.0f * 1000);
            _map.CreateJungleMonster("Wraith3.1.1", "Wraith", new Vector2(6300.05f, 5300.06f), new Vector3(6552.0f, 48.0f, 5240.0f), blueWraiths, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("LesserWraith3.1.2", "LesserWraith", new Vector2(6523.0f, 5426.95f), new Vector3(6552.0f, 48.0f, 5240.0f), blueWraiths, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("LesserWraith3.1.3", "LesserWraith", new Vector2(6653.83f, 5278.29f), new Vector3(6552.0f, 48.0f, 5240.0f), blueWraiths, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("LesserWraith3.1.4", "LesserWraith", new Vector2(6582.915f, 5107.8857f), new Vector3(6552.0f, 48.0f, 5240.0f), blueWraiths, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(blueWraiths);

            //Blue Side RedBuff
            var blue_RedBuff = _map.CreateJungleCamp(new Vector3(7455.615f, 60.0f, 3890.2026f), 4, TeamId.TEAM_BLUE, "Camp", 115.0f * 1000);
            _map.CreateJungleMonster("LizardElder4.1.1", "LizardElder", new Vector2(7455.615f, 3890.2026f), new Vector3(7348.0f, 48.0f, 3829.0f), blue_RedBuff, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("YoungLizard4.1.2", "YoungLizard", new Vector2(7460.615f, 3710.2026f), new Vector3(7348.0f, 48.0f, 3829.0f), blue_RedBuff, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("YoungLizard4.1.3", "YoungLizard", new Vector2(7237.615f, 3890.2026f), new Vector3(7348.0f, 48.0f, 3829.0f), blue_RedBuff, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(blue_RedBuff);

            //Blue Side Golems
            var blueGolems = _map.CreateJungleCamp(new Vector3(7916.8423f, 60.0f, 2533.9634f), 5, TeamId.TEAM_BLUE, "LesserCamp", 125.0f * 1000);
            _map.CreateJungleMonster("SmallGolem5.1.1", "SmallGolem", new Vector2(7916.8423f, 2533.9634f), new Vector3(7913.0f, 45.0f, 2421.0f), blueGolems, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("Golem5.1.2", "Golem", new Vector2(8216.842f, 2533.9634f), new Vector3(8163.0f, 45.0f, 2413.0f), blueGolems, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(blueGolems);

            //Dragon
            var dragon = _map.CreateJungleCamp(new Vector3(9459.52f, 60.0f, 4193.03f), 6, 0, "Dragon", 150.0f * 1000);
            _map.CreateJungleMonster("Dragon6.1.1", "Dragon", new Vector2(9459.52f, 4193.03f), new Vector3(9622.0f, -69.0f, 4490.0f), dragon, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(dragon);

            //Red Side BlueBuff
            var red_BlueBuff = _map.CreateJungleCamp(new Vector3(10386.605f, 60.0f, 6811.1123f), 7, TeamId.TEAM_PURPLE, "Camp", 115.0f * 1000);
            _map.CreateJungleMonster("AncientGolem7.1.1", "AncientGolem", new Vector2(10386.605f, 6811.1123f), new Vector3(11022.0f, 54.8568f, 6519.72f), red_BlueBuff, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("YoungLizard7.1.2", "YoungLizard", new Vector2(10586.605f, 6831.1123f), new Vector3(11022.0f, 54.8568f, 6519.72f), red_BlueBuff, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("YoungLizard4.1.3", "YoungLizard", new Vector2(10526.605f, 6601.1123f), new Vector3(11022.0f, 54.8568f, 6519.72f), red_BlueBuff, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(red_BlueBuff);

            //Red side Wolfs
            var redWolves = _map.CreateJungleCamp(new Vector3(10651.523f, 60.0f, 8116.4243f), 8, TeamId.TEAM_PURPLE, "LesserCamp", 125.0f * 1000);
            _map.CreateJungleMonster("GiantWolf8.1.1", "GiantWolf", new Vector2(10651.523f, 8116.4243f), new Vector3(10721.0f, 53.0f, 8282.0f), redWolves, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("Wolf8.1.2", "Wolf", new Vector2(10651.523f, 7916.4243f), new Vector3(10721.0f, 53.0f, 8282.0f), redWolves, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("Wolf8.1.3", "Wolf", new Vector2(10451.523f, 8116.4243f), new Vector3(10721.0f, 53.0f, 8282.0f), redWolves, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(redWolves);

            //Red Side Wraiths
            var redWraiths = _map.CreateJungleCamp(new Vector3(7580.368f, 60.0f, 9250.405f), 9, TeamId.TEAM_PURPLE, "LesserCamp", 125.0f * 1000);
            _map.CreateJungleMonster("Wraith9.1.1", "Wraith", new Vector2(7580.368f, 9250.405f), new Vector3(7495.0f, 46.0f, 9259.0f), redWraiths, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("LesserWraith9.1.2", "LesserWraith", new Vector2(7480.368f, 9091.405f), new Vector3(7495.0f, 46.0f, 9259.0f), redWraiths, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("LesserWraith9.1.3", "LesserWraith", new Vector2(7350.368f, 9230.405f), new Vector3(7495.0f, 46.0f, 9259.0f), redWraiths, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("LesserWraith9.1.4", "LesserWraith", new Vector2(7450.368f, 9350.405f), new Vector3(7495.0f, 46.0f, 9259.0f), redWraiths, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(redWraiths);

            //Red Side RedBuff
            var red_RedBuff = _map.CreateJungleCamp(new Vector3(6504.2407f, 60.0f, 10584.5625f), 10, TeamId.TEAM_PURPLE, "Camp", 115.0f * 1000);
            _map.CreateJungleMonster("LizardElder10.1.1", "LizardElder", new Vector2(6504.2407f, 10584.5625f), new Vector3(6618.0f, 45.0f, 10709.0f), red_RedBuff, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("YoungLizard10.1.2", "YoungLizard", new Vector2(6704.2407f, 10584.5625f), new Vector3(6618.0f, 45.0f, 10709.0f), red_RedBuff, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("YoungLizard10.1.3", "YoungLizard", new Vector2(6504.2407f, 10784.5625f), new Vector3(6618.0f, 45.0f, 10709.0f), red_RedBuff, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(red_RedBuff);

            //Red Side Golems
            var redGolems = _map.CreateJungleCamp(new Vector3(5810.464f, 60.0f, 11925.474f), 11, TeamId.TEAM_PURPLE, "LesserCamp", 125.0f * 1000);
            _map.CreateJungleMonster("SmallGolem11.1.1", "SmallGolem", new Vector2(5810.464f, 11925.474f), new Vector3(5859.0f, 30.0f, 12006.0f), redGolems, aiScript: "BasicJungleMonsterAi");
            _map.CreateJungleMonster("Golem11.1.2", "Golem", new Vector2(6140.464f, 11935.474f), new Vector3(6111.0f, 30.0f, 12012.0f), redGolems, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(redGolems);

            //Baron
            var baron = _map.CreateJungleCamp(new Vector3(4600.495f, 60.0f, 10250.462f), 12, 0, "Baron", 900.0f * 1000);
            _map.CreateJungleMonster("Worm12.1.1", "Worm", new Vector2(4600.495f, 10250.462f), new Vector3(4329.43f, -71.0f, 9887.0f), baron, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(baron);

            //Blue Side GreatWraith (Old gromp)
            var blueGreatGromp = _map.CreateJungleCamp(new Vector3(1684.0f, 60.0f, 8207.0f), 13, TeamId.TEAM_BLUE, "LesserCamp", 125.0f * 1000);
            _map.CreateJungleMonster("GreatWraith13.1.1", "GreatWraith", new Vector2(1684.0f, 8207.0f), new Vector3(2300.0f, 53.0f, 9720.0f), blueGreatGromp, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(blueGreatGromp);

            //Red Side GreatWraith (Old gromp)
            var redGreatGromp = _map.CreateJungleCamp(new Vector3(12337.0f, 60.0f, 6263.0f), 14, TeamId.TEAM_BLUE, "LesserCamp", 125.0f * 1000);
            _map.CreateJungleMonster("GreatWraith14.1.1", "GreatWraith", new Vector2(12337.0f, 6263.0f), new Vector3(11826.0f, 52.0f, 4788.0f), redGreatGromp, aiScript: "BasicJungleMonsterAi");
            MonsterCamps.Add(redGreatGromp);
        }

        public void SetupMapProps()
        {
            _map.AddLevelProp("LevelProp_sru_gromp_prop8", "sru_gromp_prop", new Vector3(11793.683f, 43.78664f, 7372.605f), new Vector3(359.2376f, 214.7355f, 2.0863063f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_sru_lizard10", "sru_lizard", new Vector3(10761.941f, 262.4519f, 14473.7705f), new Vector3(263.2656f, 292.26758f, 97.87612f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_sru_gromp_prop9", "sru_gromp_prop", new Vector3(9394.181f, -73.8051f, 6374.51f), new Vector3(0.0f, 94.81607f, 0.0f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_sru_snail9", "sru_snail", new Vector3(4141.109f, 97.97998f, 2237.7083f), new Vector3(97.97998f, 277.3916f, 122.50976f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_gromp_prop10", "sru_gromp_prop", new Vector3(11801.394f, 106.38264f, 6318.2627f), new Vector3(184.12091f, 346.23956f, 176.89716f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse1", "SRU_AntlerMouse", new Vector3(2213.7734f, 166.20999f, 6963.584f), new Vector3(318.9382f, 71.1862f, 332.12845f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_snail7", "sru_snail", new Vector3(4701.505f, 97.24113f, 770.36914f), new Vector3(182.573f, 353.8483f, 179.90083f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_bird1", "sru_bird", new Vector3(1507.258f, 413.54163f, -308.48724f), new Vector3(1.433609f, 46.41291f, 6.451541f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_lizard8", "sru_lizard", new Vector3(4882.003f, 209.62727f, 12030.8f), new Vector3(7.925711f, 48.344486f, 24.486755f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_sru_lizard4", "sru_lizard", new Vector3(8395.145f, 157.3293f, 10579.154f), new Vector3(353.29337f, 29.386461f, 351.035f), Vector3.Zero, new Vector3(0.7f, 0.7f, 0.7f));
            _map.AddLevelProp("LevelProp_sru_lizard4", "sru_lizard", new Vector3(5958.451f, 140.5533f, 4784.142f), new Vector3(182.24371f, 281.64584f, 178.48666f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_snail4", "sru_snail", new Vector3(5958.451f, 140.5533f, 4784.142f), new Vector3(182.24371f, 281.64584f, 178.48666f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_bird", "sru_bird", new Vector3(12418.923f, 141.62668f, 4051.2566f), new Vector3(0.0f, 160.07144f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse6", "SRU_AntlerMouse", new Vector3(2054.2656f, 50.50199f, 5703.9956f), new Vector3(348.2142f, 168.60873f, 3.6270986f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_snail6", "sru_snail", new Vector3(10269.665f, 74.85492f, 3351.2368f), new Vector3(337.66736f, 253.62976f, 18.625101f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_bird2", "sru_bird", new Vector3(147.65501f, 453.44928f, 8168.2144f), new Vector3(351.08133f, 76.557236f, 352.80908f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse11", "SRU_AntlerMouse", new Vector3(4220.001f, 43.72308f, 4499.094f), new Vector3(0.0f, 321.78326f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_bird5", "sru_bird", new Vector3(8080.158f, 468.22894f, 198.32753f), new Vector3(22.96237f, 52.549847f, 16.686834f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse9", "SRU_AntlerMouse", new Vector3(1984.2909f, 94.60245f, 4133.518f), new Vector3(0.0f, 201.94792f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_lizard3", "sru_lizard", new Vector3(6678.113f, 224.0654f, 11199.31f), new Vector3(194.6758f, 330.27f, 171.87654f), Vector3.Zero, new Vector3(0.8f, 0.8f, 0.8f));
            _map.AddLevelProp("LevelProp_sru_gromp_prop3", "sru_gromp_prop", new Vector3(10567.477f, 50.95081f, 9201.183f), new Vector3(184.27243f, 5.5906034f, 180.63428f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_sru_gromp_prop11", "sru_gromp_prop", new Vector3(14301.705f, 92.15693f, 11051.719f), new Vector3(180.44046f, 3.1205552f, 180.01422f), Vector3.Zero, new Vector3(0.5f, 0.5f, 0.5f));
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse3", "SRU_AntlerMouse", new Vector3(3803.9463f, 45.4187f, 6503.979f), new Vector3(0.0f, 189.4242f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse10", "SRU_AntlerMouse", new Vector3(1888.7023f, 95.097916f, 4102.505f), new Vector3(180.32701f, 61.95893f, 178.32957f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse4", "SRU_AntlerMouse", new Vector3(4882.856f, 46.181805f, 6505.0806f), new Vector3(0.0f, 217.368f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_gromp_prop7", "sru_gromp_prop", new Vector3(12898.956f, 43.924046f, 5151.044f), new Vector3(0.0f, 248.71564f, 0.0f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_sru_lizard7", "sru_lizard", new Vector3(7299.199f, 180.29959f, 9521.847f), new Vector3(285.42697f, 282.00763f, 84.01626f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_sru_lizard1", "sru_lizard", new Vector3(7079.7715f, 232.64514f, 12051.464f), new Vector3(345.256f, 66.61549f, 344.7693f), Vector3.Zero, new Vector3(0.8f, 0.8f, 0.8f));
            _map.AddLevelProp("LevelProp_sru_storekeepersouth", "sru_storekeepersouth", new Vector3(40.03181f, 162.70973f, 1112.4025f), new Vector3(0.0f, 225.0f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_snail", "sru_snail", new Vector3(5559.5044f, 101.42875f, 3932.0105f), new Vector3(197.56027f, 80.55253f, 199.11702f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_bird4", "sru_bird", new Vector3(14391.077f, 375.6806f, 3836.7505f), new Vector3(191.88112f, 308.0943f, 169.53096f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_gromp_prop6", "sru_gromp_prop", new Vector3(11746.374f, 43.98136f, 9005.245f), new Vector3(183.43646f, 21.18202f, 179.99998f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse8", "SRU_AntlerMouse", new Vector3(2998.508f, 100.93008f, 8743.376f), new Vector3(357.41953f, 30.51813f, 351.08383f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_SRU_storeKeeperNorth", "SRU_storeKeeperNorth", new Vector3(13727.213f, 144.67021f, 14592.507f), new Vector3(0.0f, 122.48973f, 0.0f), Vector3.Zero, new Vector3(1.6f, 1.6f, 1.6f));
            _map.AddLevelProp("LevelProp_sru_gromp_prop12", "sru_gromp_prop", new Vector3(12425.777f, 94.93915f, 10906.083f), new Vector3(344.1678f, 208.66914f, 3.5537615f), Vector3.Zero, new Vector3(0.4f, 0.4f, 0.4f));
            _map.AddLevelProp("LevelProp_sru_gromp_prop13", "sru_gromp_prop", new Vector3(14100.433f, 46.98303f, 6161.5103f), new Vector3(179.88374f, 51.788395f, 179.28593f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse", "SRU_AntlerMouse", new Vector3(5194.1357f, 123.46863f, 7012.206f), new Vector3(0.0f, 241.27545f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse5", "SRU_AntlerMouse", new Vector3(2909.064f, 47.40145f, 7627.799f), new Vector3(180.71075f, 52.327003f, 177.96869f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse2", "SRU_AntlerMouse", new Vector3(2894.0796f, 44.892128f, 5653.9956f), new Vector3(0.0f, 243.45912f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_snail1", "sru_snail", new Vector3(6533.019f, 199.94122f, 2319.8464f), new Vector3(4.0410137f, 339.42316f, 8.526711f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_gromp_prop5", "sru_gromp_prop", new Vector3(9452.197f, 214.45143f, 7685.849f), new Vector3(358.09802f, 206.08171f, 2.83302f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_sru_lizard", "sru_lizard", new Vector3(8090.797f, 186.20859f, 11305.528f), new Vector3(338.78f, 292.12982f, 6.7589006f), Vector3.Zero, new Vector3(0.8f, 0.8f, 0.8f));
            _map.AddLevelProp("LevelProp_sru_snail8", "sru_snail", new Vector3(392.39172f, 122.83496f, 1311.946f), new Vector3(176.78513f, 88.51321f, 167.40407f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_lizard2", "sru_lizard", new Vector3(9426.1875f, 134.33505f, 10994.098f), new Vector3(84.66563f, 62.788128f, 88.71596f), Vector3.Zero, new Vector3(0.8f, 0.8f, 0.8f));
            _map.AddLevelProp("LevelProp_sru_lizard11", "sru_lizard", new Vector3(9966.531f, 255.32796f, 12602.972f), new Vector3(0.0f, 0.0f, 339.353f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_sru_lizard6", "sru_lizard", new Vector3(9018.509f, 106.86509f, 12353.6455f), new Vector3(359.69028f, 206.40565f, 1.0154392f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse7", "SRU_AntlerMouse", new Vector3(2347.906f, 182.99486f, 10639.1f), new Vector3(186.76556f, 65.05644f, 184.66058f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_dragon_prop1", "sru_dragon_prop", new Vector3(-4157.4097f, -5639.8696f, 2518.217f), new Vector3(0.0f, 1.3109952f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_snail5", "sru_snail", new Vector3(6417.55f, 46.290264f, 3235.403f), new Vector3(181.94318f, 59.605614f, 179.99998f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_snail3", "sru_snail", new Vector3(9426.731f, 206.12328f, 2943.642f), new Vector3(319.2318f, 266.3502f, 12.209237f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_lizard9", "sru_lizard", new Vector3(10844.376f, 168.55817f, 12488.779f), new Vector3(345.75848f, 9.904963f, 27.804182f), Vector3.Zero, new Vector3(0.5f, 0.5f, 0.5f));
            _map.AddLevelProp("LevelProp_sru_gromp_prop", "sru_gromp_prop", new Vector3(9942.157f, 45.9148f, 8358.395f), new Vector3(180.79727f, 351.22357f, 180.09285f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_sru_dragon_prop", "sru_dragon_prop", new Vector3(-7058.438f, -10384.663f, 25191.99f), new Vector3(0.0f, 87.209694f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_lizard12", "sru_lizard", new Vector3(7564.231f, 367.14328f, 14262.364f), new Vector3(281.51004f, 281.76147f, 78.18267f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_sru_lizard8", "sru_lizard", new Vector3(4882.003f, 209.62727f, 12030.8f), new Vector3(7.925711f, 48.344486f, 24.486755f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_sru_lizard4", "sru_lizard", new Vector3(8395.145f, 157.3293f, 10579.154f), new Vector3(353.29337f, 29.386461f, 351.035f), Vector3.Zero, new Vector3(0.7f, 0.7f, 0.7f));
            _map.AddLevelProp("LevelProp_sru_snail4", "sru_snail", new Vector3(5958.451f, 140.5533f, 4784.142f), new Vector3(182.24371f, 281.64584f, 178.48666f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_bird", "sru_bird", new Vector3(12418.923f, 141.62668f, 4051.2566f), new Vector3(0.0f, 160.07144f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse6", "SRU_AntlerMouse", new Vector3(2054.2656f, 50.50199f, 5703.9956f), new Vector3(348.2142f, 168.60873f, 3.6270986f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_snail6", "sru_snail", new Vector3(10269.665f, 74.85492f, 3351.2368f), new Vector3(337.66736f, 253.62976f, 18.625101f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_bird2", "sru_bird", new Vector3(147.65501f, 453.44928f, 8168.2144f), new Vector3(351.08133f, 76.557236f, 352.80908f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse11", "SRU_AntlerMouse", new Vector3(4220.001f, 43.72308f, 4499.094f), new Vector3(0.0f, 321.78326f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_bird5", "sru_bird", new Vector3(8080.158f, 468.22894f, 198.32753f), new Vector3(22.96237f, 52.549847f, 16.686834f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse9", "SRU_AntlerMouse", new Vector3(1984.2909f, 94.60245f, 4133.518f), new Vector3(0.0f, 201.94792f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_lizard3", "sru_lizard", new Vector3(6678.113f, 224.0654f, 11199.31f), new Vector3(194.6758f, 330.27f, 171.87654f), Vector3.Zero, new Vector3(0.8f, 0.8f, 0.8f));
            _map.AddLevelProp("LevelProp_sru_gromp_prop3", "sru_gromp_prop", new Vector3(10567.477f, 50.95081f, 9201.183f), new Vector3(184.27243f, 5.5906034f, 180.63428f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_sru_gromp_prop11", "sru_gromp_prop", new Vector3(14301.705f, 92.15693f, 11051.719f), new Vector3(180.44046f, 3.1205552f, 180.01422f), Vector3.Zero, new Vector3(0.5f, 0.5f, 0.5f));
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse3", "SRU_AntlerMouse", new Vector3(3803.9463f, 45.4187f, 6503.979f), new Vector3(0.0f, 189.4242f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse10", "SRU_AntlerMouse", new Vector3(1888.7023f, 95.097916f, 4102.505f), new Vector3(180.32701f, 61.95893f, 178.32957f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse4", "SRU_AntlerMouse", new Vector3(4882.856f, 46.181805f, 6505.0806f), new Vector3(0.0f, 217.368f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_SRU_storeKeeperNorth", "SRU_storeKeeperNorth", new Vector3(13727.213f, 144.67021f, 14592.507f), new Vector3(0.0f, 122.48973f, 0.0f), Vector3.Zero, new Vector3(1.6f, 1.6f, 1.6f));
            _map.AddLevelProp("LevelProp_sru_gromp_prop12", "sru_gromp_prop", new Vector3(12425.777f, 94.93915f, 10906.083f), new Vector3(344.1678f, 208.66914f, 3.5537615f), Vector3.Zero, new Vector3(0.4f, 0.4f, 0.4f));
            _map.AddLevelProp("LevelProp_sru_gromp_prop13", "sru_gromp_prop", new Vector3(14100.433f, 46.98303f, 6161.5103f), new Vector3(179.88374f, 51.788395f, 179.28593f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse", "SRU_AntlerMouse", new Vector3(5194.1357f, 123.46863f, 7012.206f), new Vector3(0.0f, 241.27545f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse5", "SRU_AntlerMouse", new Vector3(2909.064f, 47.40145f, 7627.799f), new Vector3(180.71075f, 52.327003f, 177.96869f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse2", "SRU_AntlerMouse", new Vector3(2894.0796f, 44.892128f, 5653.9956f), new Vector3(0.0f, 243.45912f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_snail1", "sru_snail", new Vector3(6533.019f, 199.94122f, 2319.8464f), new Vector3(4.0410137f, 339.42316f, 8.526711f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_gromp_prop5", "sru_gromp_prop", new Vector3(9452.197f, 214.45143f, 7685.849f), new Vector3(358.09802f, 206.08171f, 2.83302f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_sru_lizard", "sru_lizard", new Vector3(8090.797f, 186.20859f, 11305.528f), new Vector3(338.78f, 292.12982f, 6.7589006f), Vector3.Zero, new Vector3(0.8f, 0.8f, 0.8f));
            _map.AddLevelProp("LevelProp_sru_snail8", "sru_snail", new Vector3(392.39172f, 122.83496f, 1311.946f), new Vector3(176.78513f, 88.51321f, 167.40407f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_lizard2", "sru_lizard", new Vector3(9426.1875f, 134.33505f, 10994.098f), new Vector3(84.66563f, 62.788128f, 88.71596f), Vector3.Zero, new Vector3(0.8f, 0.8f, 0.8f));
            _map.AddLevelProp("LevelProp_sru_lizard11", "sru_lizard", new Vector3(9966.531f, 255.32796f, 12602.972f), new Vector3(0.0f, 0.0f, 339.353f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_sru_lizard6", "sru_lizard", new Vector3(9018.509f, 106.86509f, 12353.6455f), new Vector3(359.69028f, 206.40565f, 1.0154392f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_SRU_AntlerMouse7", "SRU_AntlerMouse", new Vector3(2347.906f, 182.99486f, 10639.1f), new Vector3(186.76556f, 65.05644f, 184.66058f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_dragon_prop1", "sru_dragon_prop", new Vector3(-4157.4097f, -5639.8696f, 2518.21f), new Vector3(0.0f, 1.3109952f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_snail5", "sru_snail", new Vector3(6417.55f, 46.290264f, 3235.403f), new Vector3(181.94318f, 59.605614f, 179.99998f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_snail3", "sru_snail", new Vector3(9426.731f, 206.12328f, 2943.642f), new Vector3(319.2318f, 266.3502f, 12.209237f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_lizard9", "sru_lizard", new Vector3(10844.376f, 168.55817f, 12488.779f), new Vector3(345.75848f, 9.904963f, 27.804182f), Vector3.Zero, new Vector3(0.5f, 0.5f, 0.5f));
            _map.AddLevelProp("LevelProp_sru_gromp_prop", "sru_gromp_prop", new Vector3(9942.157f, 45.9148f, 8358.395f), new Vector3(180.79727f, 351.22357f, 180.09285f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
            _map.AddLevelProp("LevelProp_sru_dragon_prop", "sru_dragon_prop", new Vector3(-7058.43f, -10384.663f, 25191.99f), new Vector3(0.0f, 87.209694f, 0.0f), Vector3.Zero, Vector3.One);
            _map.AddLevelProp("LevelProp_sru_lizard12", "sru_lizard", new Vector3(7564.231f, 367.14328f, 14262.364f), new Vector3(281.51004f, 281.76147f, 78.18267f), Vector3.Zero, new Vector3(0.6f, 0.6f, 0.6f));
        }
    }
}
