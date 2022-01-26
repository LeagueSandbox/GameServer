using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Maps;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace MapScripts.Map10
{
    public class CLASSIC : IMapScript
    {
        public IMapScriptMetadata MapScriptMetadata { get; set; } = new MapScriptMetadata
        {
            EnableBuildingProtection = true,
            StartingGold = 825.0f
        };
        private IMapScriptHandler _map;
        public virtual IGlobalData GlobalData { get; set; } = new GlobalData();
        public bool HasFirstBloodHappened { get; set; } = false;
        public long NextSpawnTime { get; set; } = 45 * 1000;
        public string LaneMinionAI { get; set; } = "LaneMinionAI";
        public string LaneTurretAI { get; set; } = "TurretAI";

        public Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> PlayerSpawnPoints { get; }

        //Tower type enumeration might vary slightly from map to map, so we set that up here
        public TurretType GetTurretType(int trueIndex, LaneID lane, TeamId teamId)
        {
            TurretType returnType = TurretType.NEXUS_TURRET;
            switch (trueIndex)
            {
                case 1:
                case 6:
                case 7:
                    returnType = TurretType.INHIBITOR_TURRET;
                    break;
                case 2:
                    returnType = TurretType.INNER_TURRET;
                    break;
            }

            if (trueIndex == 1 && lane == LaneID.MIDDLE)
            {
                returnType = TurretType.NEXUS_TURRET;
            }

            return returnType;
        }

        //Nexus models
        public Dictionary<TeamId, string> NexusModels { get; set; } = new Dictionary<TeamId, string>
        {
            {TeamId.TEAM_BLUE, "OrderNexus" },
            {TeamId.TEAM_PURPLE, "ChaosNexus" }
        };
        //Inhib models
        public Dictionary<TeamId, string> InhibitorModels { get; set; } = new Dictionary<TeamId, string>
        {
            {TeamId.TEAM_BLUE, "TT_OrderInhibitor" },
            {TeamId.TEAM_PURPLE, "TT_ChaosInhibitor" }
        };
        //Tower Models
        public Dictionary<TeamId, Dictionary<TurretType, string>> TowerModels { get; set; } = new Dictionary<TeamId, Dictionary<TurretType, string>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "TT_OrderTurret4" },
                {TurretType.NEXUS_TURRET, "TT_OrderTurret3" },
                {TurretType.INHIBITOR_TURRET, "TT_OrderTurret1" },
                {TurretType.INNER_TURRET, "TT_OrderTurret2" },
            } },
            {TeamId.TEAM_PURPLE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "TT_ChaosTurret4" },
                {TurretType.NEXUS_TURRET, "TT_ChaosTurret3" },
                {TurretType.INHIBITOR_TURRET, "TT_ChaosTurret1" },
                {TurretType.INNER_TURRET, "TT_ChaosTurret2" },
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
        public void Init(IMapScriptHandler map)
        {
            _map = map;

            MapScriptMetadata.MinionSpawnEnabled = map.IsMinionSpawnEnabled();
            map.AddSurrender(1200000.0f, 300000.0f, 30.0f);

            //Due to riot's questionable map-naming scheme some towers are missplaced into other lanes during outomated setup, so we have to manually fix them.
            map.ChangeTowerOnMapList("Turret_T1_C_07_A", TeamId.TEAM_BLUE, LaneID.MIDDLE, LaneID.BOTTOM);
            map.ChangeTowerOnMapList("Turret_T1_C_06_A", TeamId.TEAM_BLUE, LaneID.MIDDLE, LaneID.TOP);

            // Announcer events
            map.AddAnnouncement(30 * 1000, EventID.OnStartGameMessage1, true); // Welcome to "Map"
            map.AddAnnouncement(NextSpawnTime - 30 * 1000, EventID.OnStartGameMessage2, true); // 30 seconds until minions spawn
            map.AddAnnouncement(NextSpawnTime, EventID.OnMinionsSpawn, false); // Minions have spawned

            //Map props
            map.AddLevelProp("LevelProp_TT_Brazier1", "TT_Brazier", new Vector2(1360.9241f, 5072.1309f), 291.2142f, new Vector3(11.1111f, 134.0f, 0.0f), new Vector3(0.0f, 288.8889f, -22.2222f), Vector3.One);
            map.AddLevelProp("LevelProp_TT_Brazier2", "TT_Brazier", new Vector2(423.5712f, 6529.0327f), 385.9983f, new Vector3(-33.3334f, 0.0f, 0.0f), new Vector3(0.0f, 277.7778f, -11.1111f), Vector3.One);
            map.AddLevelProp("LevelProp_TT_Brazier3", "TT_Brazier", new Vector2(399.4241f, 8021.057f), 692.2211f, new Vector3(-22.2222f, 0.0f, 0.0f), new Vector3(0.0f, 300f, 0.0f), Vector3.One);
            map.AddLevelProp("LevelProp_TT_Brazier4", "TT_Brazier", new Vector2(1314.294f, 9495.576f), 582.8416f, new Vector3(-33.3334f, 48.0f, 0.0f), new Vector3(0.0f, 277.7778f, 22.2223f), Vector3.One);
            map.AddLevelProp("LevelProp_TT_Brazier5", "TT_Brazier", new Vector2(14080.0f, 9530.3379f), 305.0638f, new Vector3(11.1111f, 120.0f, 0.0f), new Vector3(0.0f, 277.7778f, 0.0f), Vector3.One);
            map.AddLevelProp("LevelProp_TT_Brazier6", "TT_Brazier", new Vector2(14990.46f, 8053.91f), 675.8145f, new Vector3(-22.2222f, 0.0f, 0.0f), new Vector3(0.0f, 266.6666f, -11.1111f), Vector3.One);
            map.AddLevelProp("LevelProp_TT_Brazier7", "TT_Brazier", new Vector2(15016.35f, 6532.84f), 664.7033f, new Vector3(-11.1111f, 0.0f, 0.0f), new Vector3(0.0f, 255.5555f, -11.1111f), Vector3.One);
            map.AddLevelProp("LevelProp_TT_Brazier8", "TT_Brazier", new Vector2(14102.99f, 5098.367f), 580.504f, new Vector3(0.0f, 36.0f, 0.0f), new Vector3(0.0f, 244.4445f, 11.1111f), Vector3.One);
            map.AddLevelProp("LevelProp_TT_Chains_Bot_Lane", "TT_Chains_Bot_Lane", new Vector2(3624.281f, 3730.965f), -100.4387f, new Vector3(88.8889f, 0.0f, 0.0f), new Vector3(0.0f, -33.3334f, 66.6667f), Vector3.One);
            map.AddLevelProp("LevelProp_TT_Chains_Order_Base", "TT_Chains_Order_Base", new Vector2(3778.364f, 7573.525f), -496.0713f, new Vector3(-233.3334f, 0.0f, 0.0f), new Vector3(0.0f, -333.3333f, 277.7778f), Vector3.One);
            map.AddLevelProp("LevelProp_TT_Chains_Xaos_Base", "TT_Chains_Xaos_Base", new Vector2(11636.06f, 7618.667f), -551.6268f, new Vector3(200.0f, 0.0f, 0.0f), new Vector3(0.0f, -388.8889f, 33.3334f), Vector3.One);
            map.AddLevelProp("LevelProp_TT_Chains_Order_Periph", "TT_Chains_Order_Periph", new Vector2(759.1779f, 4740.938f), 507.9883f, new Vector3(-155.5555f, 0.0f, 0.0f), new Vector3(0.0f, 44.4445f, 222.2222f), Vector3.One);
            map.AddLevelProp("LevelProp_TT_Nexus_Gears", "TT_Nexus_Gears", new Vector2(3000.0f, 7289.682f), 19.51249f, Vector3.Zero, new Vector3(0.0f, 144.4445f, 0.0f), Vector3.One);
            map.AddLevelProp("LevelProp_TT_Nexus_Gears1", "TT_Nexus_Gears", new Vector2(12436.4775f, 7366.5859f), -124.9320f, new Vector3(-44.4445f, 180.0f, 0.0f), new Vector3(0.0f, 122.2222f, -122.2222f), Vector3.One);
            map.AddLevelProp("LevelProp_TT_Shopkeeper1", "TT_Shopkeeper", new Vector2(14169.09f, 7916.989f), 178.1922f, new Vector3(22.2223f, 150f, 0.0f), new Vector3(33.3333f, 0.0f, -66.6667f), Vector3.One);
            map.AddLevelProp("LevelProp_TT_Shopkeeper", "TT_Shopkeeper", new Vector2(1340.8141f, 7996.8691f), 126.2980f, new Vector3(208f, -66.6667f, 0.0f), new Vector3(0.0f, 22.2223f, -55.5556f), Vector3.One);
            map.AddLevelProp("LevelProp_TT_Speedshrine_Gears", "TT_Speedshrine_Gears", new Vector2(7706.3052f, 6720.3926f), -124.9320f, Vector3.Zero, Vector3.Zero, Vector3.One);
        }
        public void OnMatchStart()
        {
            foreach (var nexus in _map.NexusList)
            {
                ApiEventManager.OnDeath.AddListener(this, nexus, OnNexusDeath, true);
            }
        }

        //This function gets executed every server tick
        public void Update(float diff)
        {
        }

        public void OnNexusDeath(IDeathData deathaData)
        {
            var nexus = deathaData.Unit;
            _map.EndGame(nexus.Team, new Vector3(nexus.Position.X, nexus.GetHeight(), nexus.Position.Y), deathData: deathaData);
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
