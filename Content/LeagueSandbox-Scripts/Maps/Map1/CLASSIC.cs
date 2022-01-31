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

namespace MapScripts.Map1
{
    public class CLASSIC : IMapScript
    {
        public virtual IMapScriptMetadata MapScriptMetadata { get; set; } = new MapScriptMetadata
        {
            MinionPathingOverride = true,
            EnableBuildingProtection = true
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
            {TeamId.TEAM_BLUE, "OrderNexus" },
            {TeamId.TEAM_PURPLE, "ChaosNexus" }
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

            //Due to riot's questionable map-naming scheme some towers are missplaced into other lanes during outomated setup, so we have to manually fix them.
            map.ChangeTowerOnMapList("Turret_T1_C_06_A", TeamId.TEAM_BLUE, LaneID.MIDDLE, LaneID.TOP);
            map.ChangeTowerOnMapList("Turret_T1_C_07_A", TeamId.TEAM_BLUE, LaneID.MIDDLE, LaneID.BOTTOM);

            // Announcer events

            // Welcome to "Map"
            map.AddAnnouncement(30 * 1000, EventID.OnStartGameMessage1, true);
            // 30 seconds until minions spawn
            map.AddAnnouncement(NextSpawnTime - 30 * 1000, EventID.OnStartGameMessage2, true);
            // Minions have spawned
            map.AddAnnouncement(NextSpawnTime, EventID.OnMinionsSpawn, false);

            //Map props
            _map.AddLevelProp("LevelProp_Yonkey", "Yonkey", new Vector2(12465.0f, 14422.257f), 101.0f, new Vector3(0.0f, 66.0f, 0.0f), new Vector3(-33.3334f, 122.2222f, -133.3333f), Vector3.One);
            _map.AddLevelProp("LevelProp_Yonkey1", "Yonkey", new Vector2(-76.0f, 1769.1589f), 94.0f, new Vector3(0.0f, 30.0f, 0.0f), new Vector3(0.0f, -11.1111f, -22.2222f), Vector3.One);
            _map.AddLevelProp("LevelProp_ShopMale", "ShopMale", new Vector2(13374.17f, 14245.673f), 194.9741f, new Vector3(0.0f, 224f, 0.0f), new Vector3(0.0f, 33.3333f, -44.4445f), Vector3.One);
            _map.AddLevelProp("LevelProp_ShopMale1", "ShopMale", new Vector2(-99.5613f, 855.6632f), 191.4039f, new Vector3(0.0f, 158.0f, 0.0f), Vector3.Zero, Vector3.One);
        }

        List<IMonsterCamp> MonsterCamps = new List<IMonsterCamp>();
        public virtual void OnMatchStart()
        {
            foreach (var nexus in _map.NexusList)
            {
                ApiEventManager.OnDeath.AddListener(this, nexus, OnNexusDeath, true);
            }
            SetupJungleCamps();
        }

        public void Update(float diff)
        {
            if (_map.GameTime() >= 120 * 1000)
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
    }
}
