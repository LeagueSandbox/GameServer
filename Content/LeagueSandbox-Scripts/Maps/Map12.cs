using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Maps;
using LeagueSandbox.GameServer.Logging;
using log4net;

namespace MapScripts
{
    public class Map12 : IMapScript
    {
        public bool EnableBuildingProtection { get; set; } = true;

        //General Map variable
        private IMap _map;
        private static ILog _logger = LoggerProvider.GetLogger();

        //Stuff about minions
        public bool SpawnEnabled { get; set; }
        public long FirstSpawnTime { get; set; } = 45 * 1000;
        public long NextSpawnTime { get; set; } = 45 * 1000;
        public long SpawnInterval { get; set; } = 30 * 1000;
        public bool MinionPathingOverride { get; set; } = false;

        //General things that will affect players globaly, such as default gold per-second, Starting gold....
        public float GoldPerSecond { get; set; } = 5.0f;
        public float StartingGold { get; set; } = 1375.0f;
        public bool HasFirstBloodHappened { get; set; } = false;
        public bool IsKillGoldRewardReductionActive { get; set; } = true;
        public int BluePillId { get; set; } = 2001;
        public long FirstGoldTime { get; set; } = 90 * 1000;

        //Tower type enumeration might vary slightly from map to map, so we set that up here
        public TurretType GetTurretType(int trueIndex, LaneID lane, TeamId teamId)
        {
            TurretType returnType = TurretType.FOUNTAIN_TURRET;

            switch (trueIndex)
            {
                case 3:
                case 4:
                case 9:
                case 10:
                    returnType = TurretType.NEXUS_TURRET;
                    break;
                case 2:
                case 7:
                    returnType = TurretType.INHIBITOR_TURRET;
                    break;
                case 1:
                case 8:
                    returnType = TurretType.INNER_TURRET;
                    break;
            }

            return returnType;
        }

        //Nexus models
        public Dictionary<TeamId, string> NexusModels { get; set; } = new Dictionary<TeamId, string>
        {
            {TeamId.TEAM_BLUE, "ARAMOrderNexus" },
            {TeamId.TEAM_PURPLE, "ARAMChaosNexus" }
        };
        //Inhib models
        public Dictionary<TeamId, string> InhibitorModels { get; set; } = new Dictionary<TeamId, string>
        {
            {TeamId.TEAM_BLUE, "OrderInhibitor" },
            {TeamId.TEAM_PURPLE, "ChaosInhibitor" }
        };
        //Tower Models
        public Dictionary<TeamId, Dictionary<TurretType, string>> TowerModels { get; set; } = new Dictionary<TeamId, Dictionary<TurretType, string>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "HA_AP_OrderShrineTurret" },
                {TurretType.NEXUS_TURRET, "HA_AP_OrderTurret" },
                {TurretType.INHIBITOR_TURRET, "HA_AP_OrderTurret2" },
                {TurretType.OUTER_TURRET, "HA_AP_OrderTurret3" },
            } },
            {TeamId.TEAM_PURPLE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "HA_AP_ChaosTurretShrine" },
                {TurretType.NEXUS_TURRET, "HA_AP_ChaosTurret" },
                {TurretType.INHIBITOR_TURRET, "HA_AP_ChaosTurret2" },
                {TurretType.OUTER_TURRET, "HA_AP_ChaosTurret3" },
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

        public Dictionary<LaneID, List<Vector2>> MinionPaths { get; set; }
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
        public void Init(IMap map)
        {
            _map = map;

            SpawnEnabled = map.IsMinionSpawnEnabled();
            map.AddSurrender(1200000.0f, 300000.0f, 30.0f);

            //Due to riot's questionable map-naming scheme some towers are missplaced into other lanes during outomated setup, so we have to manually fix them.
            map.ChangeTowerOnMapList("Turret_T2_L_04_A", TeamId.TEAM_PURPLE, LaneID.TOP, LaneID.MIDDLE);
            map.ChangeTowerOnMapList("Turret_T2_L_01_A", TeamId.TEAM_PURPLE, LaneID.TOP, LaneID.MIDDLE);
            map.ChangeTowerOnMapList("Turret_T2_L_02_A", TeamId.TEAM_PURPLE, LaneID.TOP, LaneID.MIDDLE);
            map.ChangeTowerOnMapList("Turret_T2_L_03_A", TeamId.TEAM_PURPLE, LaneID.TOP, LaneID.MIDDLE);

            // Announcer events
            map.AddAnnouncement(FirstSpawnTime - 30 * 1000, Announces.THIRY_SECONDS_TO_MINIONS_SPAWN, true); // 30 seconds until minions spawn
            map.AddAnnouncement(FirstSpawnTime, Announces.MINIONS_HAVE_SPAWNED, false); // Minions have spawned (90 * 1000)
            map.AddAnnouncement(FirstSpawnTime, Announces.MINIONS_HAVE_SPAWNED2, false); // Minions have spawned [2] (90 * 1000)

            // Level Props
            _map.AddLevelProp("LevelProp_HA_AP_HeroTower", "HA_AP_HeroTower", new Vector2(1637.6909f, 6079.676f), -3986.0718f, new Vector3(0f, 316f, 0f), new Vector3(0f, -1000f, 0f), Vector3.One, type: 2);
            //_map.AddLevelProp("LevelProp_HA_AP_Chains_Long", "HA_AP_Chains_Long", new Vector2(2883.2095f, 5173.606f), 86.12982f, new Vector3(0f, 324f, 0f), new Vector3(-88.8889f, 355.5555f, -100.0f), Vector3.One, type: 2);
            _map.AddLevelProp("LevelProp_HA_AP_BridgeLaneStatue1", "HA_AP_BridgeLaneStatue", new Vector2(4904.28f, 3607.452f), 92.056755f, new Vector3(0f, 134f, 0f), new Vector3(-122.2222f, 177.7777f, 111.1112f), Vector3.One, type: 2);
            _map.AddLevelProp("LevelProp_HA_AP_Poro4", "HA_AP_Poro", new Vector2(10655.31f, 8387.441f), -375.02237f, new Vector3(0f, 222f, 0f), new Vector3(266.6666f, -55.5556f, -11.1111f), Vector3.One);
            //_map.AddLevelProp("LevelProp_HA_AP_Chains_Long2", "HA_AP_Chains_Long", new Vector2(5139.6133f, 2801.751f), 97.240906f, new Vector3(0f, 314f, 0f), new Vector3(155.5557f, 366.6666f, -322.2222f), Vector3.One, type: 2);
            _map.AddLevelProp("LevelProp_HA_AP_Hermit", "HA_AP_Hermit", new Vector2(11218.746f, 12037.217f), -164.43712f, new Vector3(0f, 136f, 0.0f), new Vector3(88.8889f, 44.4445f, 30.0f), Vector3.One);
            _map.AddLevelProp("LevelProp_HA_AP_Chains6", "HA_AP_Chains", new Vector2(6074.8384f, 3868.2524f), 87.96773f, new Vector3(0f, 318f, 0.0f), new Vector3(-77.7778f, 222.2222f, -44.4445f), Vector3.One);
            _map.AddLevelProp("LevelProp_HA_AP_Chains4", "HA_AP_Chains", new Vector2(7492.48f, 5250.153f), 76.85663f, new Vector3(0f, 320f, 0.0f), new Vector3(-22.2222f, 211.1111f, 22.2223f), Vector3.One);
            _map.AddLevelProp("LevelProp_HA_AP_BridgeLaneStatue4", "HA_AP_BridgeLaneStatue", new Vector2(9189.304f, 7740.729f), 79.74847f, new Vector3(0f, 134f, 0f), new Vector3(-133.3333f, 144.4445f, 122.2222f), Vector3.One, type: 2);
            _map.AddLevelProp("LevelProp_HA_AP_BridgeLaneStatue2", "HA_AP_BridgeLaneStatue", new Vector2(6321.7275f, 5004.743f), 88.65647f, new Vector3(0f, 134f, 0f), new Vector3(-144.4445f, 155.5557f, 144.4445f), Vector3.One, type: 2);
            _map.AddLevelProp("LevelProp_HA_AP_ShpNorth", "HA_AP_ShpNorth", new Vector2(10811.736f, 11978.403f), -217.8545f, new Vector3(0.0f, 292f, 0.0f), new Vector3(-211.1111f, -144.4445f, -111.1111f), Vector3.One);
            _map.AddLevelProp("LevelProp_HA_AP_Poro2", "HA_AP_Poro", new Vector2(5644.625f, 7807.1074f), -732.70325f, new Vector3(0f, 34f, 0f), new Vector3(-88.8889f, 0f, 0f), Vector3.One);
            //_map.AddLevelProp("LevelProp_HA_AP_Chains_Long3", "HA_AP_Chains_Long", new Vector2(7724.4653f, 9869.202f), 86.12982f, new Vector3(0f, 314f, 0f), new Vector3(-33.3334f, 355.5555f, 166.6666f), Vector3.One, type: 2);
            _map.AddLevelProp("LevelProp_HA_AP_ShpSouth", "HA_AP_ShpSouth", new Vector2(521.2136f, 1913.0146f), -186.65932f, new Vector3(0f, 316f, 0.0f), new Vector3(66.6667f, 22.2223f, 11.1111f), Vector3.One);
            _map.AddLevelProp("LevelProp_HA_AP_BridgeLaneStatue5", "HA_AP_BridgeLaneStatue", new Vector2(7918.951f, 9124.248f), 77.485504f, new Vector3(0f, 316f, 0f), new Vector3(111.1112f, 144.4445f, -111.1111f), Vector3.One, type: 2);
            _map.AddLevelProp("LevelProp_HA_AP_Poro3", "HA_AP_Poro", new Vector2(11036.813f, 12432.2109f), -732.7032f, new Vector3(0f, 166f, 0f), new Vector3(44.4445f, 0f, 0f), Vector3.One);
            //_map.AddLevelProp("LevelProp_HA_AP_Chains_Long1", "HA_AP_Chains_Long", new Vector2(9939.937f, 7628.735f), 69.55461f, new Vector3(0f, 320f, 0f), new Vector3(111.1112f, 300.0f, -111.1111f), Vector3.One, type: 2);
            _map.AddLevelProp("LevelProp_HA_AP_Poro6", "HA_AP_Poro", new Vector2(6753.814f, 5412.7236f), 48.33051f, new Vector3(0f, 130f, 0f), new Vector3(-22.2222f, -11.1111f, 0f), Vector3.One);
            _map.AddLevelProp("LevelProp_HA_AP_Cutaway", "HA_AP_Cutaway", new Vector2(7815.47f, 7517.7188f), -222.17885f, new Vector3(0f, 314f, 0f), new Vector3(-722.2222f, 177.7777f, 244.4445f), Vector3.One);
            _map.AddLevelProp("LevelProp_HA_AP_BridgeLaneStatue8", "HA_AP_BridgeLaneStatue", new Vector2(3633.1042f, 4999.506f), 87.18081f, new Vector3(0f, 316f, 0f), new Vector3(144.4445f, 155.5557f, -144.4445f), Vector3.One, type: 2);
            _map.AddLevelProp("LevelProp_HA_AP_BridgeLaneStatue7", "HA_AP_BridgeLaneStatue", new Vector2(5055.8486f, 6352.3774f), 86.278946f, new Vector3(0f, 318f, 0f), new Vector3(144.4445f, 155.5557f, -166.6667f), Vector3.One, type: 2);
            _map.AddLevelProp("LevelProp_HA_AP_PeriphBridge", "HA_AP_PeriphBridge", new Vector2(-500.19504f, 17371.6f), -8219.082f, new Vector3(0f, 334f, 0f), new Vector3(-611.1111f, 322.2223f, 88.8889f), Vector3.One, type: 2);
            _map.AddLevelProp("LevelProp_HA_AP_Viking", "HA_AP_Viking", new Vector2(515.8099f, 1919.1678f), -97.770424f, new Vector3(0f, 130f, 0.0f), new Vector3(77.7777f, 111.1112f, 22.2223f), Vector3.One);
            _map.AddLevelProp("LevelProp_HA_AP_BridgeLaneStatue3", "HA_AP_BridgeLaneStatue", new Vector2(7777.0776f, 6377.4634f), 84.44989f, new Vector3(0f, 136f, 0f), new Vector3(-122.2222f, 155.5557f, 133.3334f), Vector3.One, type: 2);
            _map.AddLevelProp("LevelProp_HA_AP_BridgeLaneStatue6", "HA_AP_BridgeLaneStatue", new Vector2(6489.379f, 7746.419f), 88.206276f, new Vector3(0f, 316f, 0f), new Vector3(144.4445f, 155.5557f, -155.5555f), Vector3.One, type: 2);
            _map.AddLevelProp("LevelProp_HA_AP_Chains1", "HA_AP_Chains", new Vector2(3931.2275f, 6102.559f), 87.96773f, new Vector3(0f, 314f, 0.0f), new Vector3(-22.2222f, 222.2222f, -66.6667f), Vector3.One);
            _map.AddLevelProp("LevelProp_HA_AP_Poro", "HA_AP_Poro", new Vector2(411.7452f, 515.7069f), 751.8175f, new Vector3(6f, 136f, 8f), new Vector3(-44.4445f, -11.1111f, -77.7778f), Vector3.One);
            _map.AddLevelProp("LevelProp_HA_AP_Hermit_Robot1", "HA_AP_Hermit_Robot", new Vector2(11196.524f, 12129.439f), -208.88162f, new Vector3(0f, 160f, 0.0f), new Vector3(66.6667f, 0.0f, 122.2222f), Vector3.One);
            _map.AddLevelProp("LevelProp_HA_AP_BannerMidBridge", "HA_AP_BannerMidBridge", new Vector2(7088.8677f, 5605.5967f), -757.4999f, new Vector3(0f, 316f, 0.0f), new Vector3(-44.4445f, 111.1112f, 33.3333f), Vector3.One);
            _map.AddLevelProp("LevelProp_HA_AP_Chains5", "HA_AP_Chains", new Vector2(8959.606f, 6577.5415f), 87.96773f, new Vector3(0f, 316f, 0.0f), new Vector3(-33.3334f, 222.2222f, -77.7778f), Vector3.One);
            _map.AddLevelProp("LevelProp_HA_AP_Poro5", "HA_AP_Poro", new Vector2(12556.904f, 9949.372f), -861.41876f, new Vector3(0f, 182f, 0f), new Vector3(-288.8889f, -22.2222f, 244.4445f), Vector3.One);
            _map.AddLevelProp("LevelProp_HA_AP_Chains3", "HA_AP_Chains", new Vector2(6770.9233f, 8888.638f), 65.74553f, new Vector3(0f, 316f, 0.0f), new Vector3(-33.3334f, 200.0f, -33.3334f), Vector3.One);
            _map.AddLevelProp("LevelProp_HA_AP_Chains2", "HA_AP_Chains", new Vector2(5348.3027f, 7505.369f), 76.85663f, new Vector3(0f, 316f, 0.0f), new Vector3(11.1111f, 211.1111f, 0f), Vector3.One);
            _map.AddLevelProp("LevelProp_HA_AP_Poro1", "HA_AP_Poro", new Vector2(2141.1174f, 4335.2715f), -113.360855f, new Vector3(0f, 208f, 0f), new Vector3(-333.3333f, -55.5556f, 0f), Vector3.One);
        }

        public void OnMatchStart()
        {
        }

        //This function gets executed every server tick
        public void Update(float diff)
        {
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
    }
}
