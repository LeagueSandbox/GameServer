using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace LeagueSandbox.GameServer.Maps
{
    internal class SummonersRift : IMapProperties
    {
        private static readonly List<Vector2> BlueTopWaypoints = new List<Vector2>
        {
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
            new Vector2(12511.0f, 12776.0f)
        };
        private static readonly List<Vector2> BlueBotWaypoints = new List<Vector2>
        {
            new Vector2(1487.0f, 1302.0f),
            new Vector2(3789.0f, 1346.0f),
            new Vector2(6430.0f, 1005.0f),
            new Vector2(10995.0f, 1234.0f),
            new Vector2(12841.0f, 3051.0f),
            new Vector2(13148.0f, 4202.0f),
            new Vector2(13249.0f, 7884.0f),
            new Vector2(12886.0f, 10356.0f),
            new Vector2(12511.0f, 12776.0f)
        };
        private static readonly List<Vector2> BlueMidWaypoints = new List<Vector2>
        {
            new Vector2(1418.0f, 1686.0f),
            new Vector2(2997.0f, 2781.0f),
            new Vector2(4472.0f, 4727.0f),
            new Vector2(8375.0f, 8366.0f),
            new Vector2(10948.0f, 10821.0f),
            new Vector2(12511.0f, 12776.0f)
        };
        private static readonly List<Vector2> RedTopWaypoints = new List<Vector2>
        {
            new Vector2(12451.0f, 13217.0f),
            new Vector2(10947.0f, 13135.0f),
            new Vector2(10244.0f, 13238.0f),
            new Vector2(7550.0f, 13407.0f),
            new Vector2(3907.0f, 13243.0f),
            new Vector2(2806.0f, 13075.0f),
            new Vector2(1268.0f, 11675.0f),
            new Vector2(880.0f, 10180.0f),
            new Vector2(861.0f, 6459.0f),
            new Vector2(1170.0f, 4041.0f),
            new Vector2(1418.0f, 1686.0f)
        };
        private static readonly List<Vector2> RedBotWaypoints = new List<Vector2>
        {
            new Vector2(13062.0f, 12760.0f),
            new Vector2(12886.0f, 10356.0f),
            new Vector2(13249.0f, 7884.0f),
            new Vector2(13148.0f, 4202.0f),
            new Vector2(12841.0f, 3051.0f),
            new Vector2(10995.0f, 1234.0f),
            new Vector2(6430.0f, 1005.0f),
            new Vector2(3789.0f, 1346.0f),
            new Vector2(1418.0f, 1686.0f)
        };
        private static readonly List<Vector2> RedMidWaypoints = new List<Vector2>
        {
            new Vector2(12511.0f, 12776.0f),
            new Vector2(10948.0f, 10821.0f),
            new Vector2(8375.0f, 8366.0f),
            new Vector2(4472.0f, 4727.0f),
            new Vector2(2997.0f, 2781.0f),
            new Vector2(1418.0f, 1686.0f)
        };

        private static readonly List<MinionSpawnType> RegularMinionWave = new List<MinionSpawnType>
        {
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER
        };
        private static readonly List<MinionSpawnType> CannonMinionWave = new List<MinionSpawnType>
        {
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CANNON,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER
        };
        private static readonly List<MinionSpawnType> SuperMinionWave = new List<MinionSpawnType>
        {
            MinionSpawnType.MINION_TYPE_SUPER,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER
        };
        private static readonly List<MinionSpawnType> DoubleSuperMinionWave = new List<MinionSpawnType>
        {
            MinionSpawnType.MINION_TYPE_SUPER,
            MinionSpawnType.MINION_TYPE_SUPER,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER
        };

        private static readonly Dictionary<TeamId, Vector3> EndGameCameraPosition = new Dictionary<TeamId, Vector3>
        {
            { TeamId.TEAM_BLUE, new Vector3(1170, 1470, 188) },
            { TeamId.TEAM_PURPLE, new Vector3(12800, 13100, 110) }
        };

        private static readonly Dictionary<TeamId, ITarget> SpawnsByTeam = new Dictionary<TeamId, ITarget>
        {
            {TeamId.TEAM_BLUE, new Target(25.90f, 280)},
            {TeamId.TEAM_PURPLE, new Target(13948, 14202)}
        };

        private static readonly Dictionary<TurretType, int[]> TurretItems = new Dictionary<TurretType, int[]>
        {
            { TurretType.OUTER_TURRET, new[] { 1500, 1501, 1502, 1503 } },
            { TurretType.INNER_TURRET, new[] { 1500, 1501, 1502, 1503, 1504 } },
            { TurretType.INHIBITOR_TURRET, new[] { 1501, 1502, 1503, 1505 } },
            { TurretType.NEXUS_TURRET, new[] { 1501, 1502, 1503, 1505 } }
        };


        private Game _game;
        private int _cannonMinionCount;
        private int _minionNumber;
        private readonly long _firstSpawnTime = 90 * 1000;
        private long _nextSpawnTime = 90 * 1000;
        private readonly long _spawnInterval = 30 * 1000;
        private readonly Dictionary<TeamId, Fountain> _fountains;

        public List<int> ExpToLevelUp { get; set; } = new List<int>
        {
            0,
            280,
            660,
            1140,
            1720,
            2400,
            3180,
            4060,
            5040,
            6120,
            7300,
            8580,
            9960,
            11440,
            13020,
            14700,
            16480,
            18360
        };

        public float GoldPerSecond { get; set; } = 1.9f;
        public float StartingGold { get; set; } = 475.0f;
        public bool HasFirstBloodHappened { get; set; } = false;
        public bool IsKillGoldRewardReductionActive { get; set; } = true;
        public int BluePillId { get; set; } = 2001;
        public long FirstGoldTime { get; set; } = 90 * 1000;
        public bool SpawnEnabled { get; set; }
        public SummonersRift(Game game)
        {
            _game = game;
            _fountains = new Dictionary<TeamId, Fountain>
            {
                { TeamId.TEAM_BLUE, new Fountain(game, TeamId.TEAM_BLUE, 11, 250, 1000) },
                { TeamId.TEAM_PURPLE, new Fountain(game, TeamId.TEAM_PURPLE, 13950, 14200, 1000) }
            };
            SpawnEnabled = _game.Config.MinionSpawnsEnabled;
        }

        public int[] GetTurretItems(TurretType type)
        {
            if (!TurretItems.ContainsKey(type))
            {
                return null;
            }

            return TurretItems[type];
        }

        public void Init()
        {
            // Announcer events
            _game.Map.AnnouncerEvents.Add(new Announce(_game, 30 * 1000, Announces.WELCOME_TO_SR, true)); // Welcome to SR
            if (_firstSpawnTime - 30 * 1000 >= 0.0f)
                _game.Map.AnnouncerEvents.Add(new Announce(_game, _firstSpawnTime - 30 * 1000, Announces.THIRY_SECONDS_TO_MINIONS_SPAWN, true)); // 30 seconds until minions spawn
            _game.Map.AnnouncerEvents.Add(new Announce(_game, _firstSpawnTime, Announces.MINIONS_HAVE_SPAWNED, false)); // Minions have spawned (90 * 1000)
            _game.Map.AnnouncerEvents.Add(new Announce(_game, _firstSpawnTime, Announces.MINIONS_HAVE_SPAWNED2, false)); // Minions have spawned [2] (90 * 1000)

            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_R_03_A", 10097.62f, 808.73f, TeamId.TEAM_BLUE,
                TurretType.OUTER_TURRET, GetTurretItems(TurretType.OUTER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_R_02_A", 6512.53f, 1262.62f, TeamId.TEAM_BLUE,
                TurretType.INNER_TURRET, GetTurretItems(TurretType.INNER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_C_07_A", 3747.26f, 1041.04f, TeamId.TEAM_BLUE,
                TurretType.INHIBITOR_TURRET, GetTurretItems(TurretType.INHIBITOR_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_R_03_A", 13459.0f, 4284.0f, TeamId.TEAM_PURPLE,
                TurretType.OUTER_TURRET, GetTurretItems(TurretType.OUTER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_R_02_A", 12920.0f, 8005.0f, TeamId.TEAM_PURPLE,
                TurretType.INNER_TURRET, GetTurretItems(TurretType.INNER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_R_01_A", 13205.0f, 10474.0f, TeamId.TEAM_PURPLE,
                TurretType.INHIBITOR_TURRET, GetTurretItems(TurretType.INHIBITOR_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_C_05_A", 5448.02f, 6169.10f, TeamId.TEAM_BLUE,
                TurretType.OUTER_TURRET, GetTurretItems(TurretType.OUTER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_C_04_A", 4657.66f, 4591.91f, TeamId.TEAM_BLUE,
                TurretType.INNER_TURRET, GetTurretItems(TurretType.INNER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_C_03_A", 3233.99f, 3447.24f, TeamId.TEAM_BLUE,
                TurretType.INHIBITOR_TURRET, GetTurretItems(TurretType.INHIBITOR_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_C_01_A", 1341.63f, 2029.98f, TeamId.TEAM_BLUE,
                TurretType.NEXUS_TURRET, GetTurretItems(TurretType.NEXUS_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_C_02_A", 1768.19f, 1589.47f, TeamId.TEAM_BLUE,
                TurretType.NEXUS_TURRET, GetTurretItems(TurretType.NEXUS_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_C_05_A", 8548.0f, 8289.0f, TeamId.TEAM_PURPLE,
                TurretType.OUTER_TURRET, GetTurretItems(TurretType.OUTER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_C_04_A", 9361.0f, 9892.0f, TeamId.TEAM_PURPLE,
                TurretType.INNER_TURRET, GetTurretItems(TurretType.INNER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_C_03_A", 10743.0f, 11010.0f, TeamId.TEAM_PURPLE,
                TurretType.INHIBITOR_TURRET, GetTurretItems(TurretType.INHIBITOR_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_C_01_A", 12662.0f, 12442.0f, TeamId.TEAM_PURPLE,
                TurretType.NEXUS_TURRET, GetTurretItems(TurretType.NEXUS_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_C_02_A", 12118.0f, 12876.0f, TeamId.TEAM_PURPLE,
                TurretType.NEXUS_TURRET, GetTurretItems(TurretType.NEXUS_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_OrderTurretShrine_A", -236.05f, -53.32f, TeamId.TEAM_BLUE,
                TurretType.FOUNTAIN_TURRET, GetTurretItems(TurretType.FOUNTAIN_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_ChaosTurretShrine_A", 14157.0f, 14456.0f, TeamId.TEAM_PURPLE,
                TurretType.FOUNTAIN_TURRET, GetTurretItems(TurretType.FOUNTAIN_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_L_03_A", 574.66f, 10220.47f, TeamId.TEAM_BLUE,
                TurretType.OUTER_TURRET, TurretItems[TurretType.OUTER_TURRET]));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_L_02_A", 1106.26f, 6485.25f, TeamId.TEAM_BLUE,
                TurretType.INNER_TURRET, GetTurretItems(TurretType.INNER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_C_06_A", 802.81f, 4052.36f, TeamId.TEAM_BLUE,
                TurretType.INHIBITOR_TURRET, GetTurretItems(TurretType.INHIBITOR_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_L_03_A", 3911.0f, 13654.0f, TeamId.TEAM_PURPLE,
                TurretType.OUTER_TURRET, TurretItems[TurretType.OUTER_TURRET]));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_L_02_A", 7536.0f, 13190.0f, TeamId.TEAM_PURPLE,
                TurretType.INNER_TURRET, GetTurretItems(TurretType.INNER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_L_01_A", 10261.0f, 13465.0f, TeamId.TEAM_PURPLE,
                TurretType.INHIBITOR_TURRET, GetTurretItems(TurretType.INHIBITOR_TURRET)));

            _game.ObjectManager.AddObject(new LevelProp(_game, 12465.0f, 14422.257f, 101.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, "LevelProp_Yonkey", "Yonkey"));
            _game.ObjectManager.AddObject(new LevelProp(_game, -76.0f, 1769.1589f, 94.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, "LevelProp_Yonkey1", "Yonkey"));
            _game.ObjectManager.AddObject(new LevelProp(_game, 13374.17f, 14245.673f, 194.9741f, 224.0f, 33.33f, 0.0f, 0.0f, -44.44f, "LevelProp_ShopMale", "ShopMale"));
            _game.ObjectManager.AddObject(new LevelProp(_game, -99.5613f, 855.6632f, 191.4039f, 158.0f, 0.0f, 0.0f, 0.0f, 0.0f, "LevelProp_ShopMale1", "ShopMale"));

            //TODO
            var collisionRadius = 0;
            var sightRange = 1700;

            _game.ObjectManager.AddObject(new Inhibitor(_game, "OrderInhibitor", TeamId.TEAM_BLUE, collisionRadius, 835, 3400, sightRange, 0xffd23c3e)); //top
            _game.ObjectManager.AddObject(new Inhibitor(_game, "OrderInhibitor", TeamId.TEAM_BLUE, collisionRadius, 2785, 3000, sightRange, 0xff4a20f1)); //mid
            _game.ObjectManager.AddObject(new Inhibitor(_game, "OrderInhibitor", TeamId.TEAM_BLUE, collisionRadius, 3044, 1070, sightRange, 0xff9303e1)); //bot
            _game.ObjectManager.AddObject(new Inhibitor(_game, "ChaosInhibitor", TeamId.TEAM_PURPLE, collisionRadius, 10960, 13450, sightRange, 0xff6793d0)); //top
            _game.ObjectManager.AddObject(new Inhibitor(_game, "ChaosInhibitor", TeamId.TEAM_PURPLE, collisionRadius, 11240, 11490, sightRange, 0xffff8f1f)); //mid
            _game.ObjectManager.AddObject(new Inhibitor(_game, "ChaosInhibitor", TeamId.TEAM_PURPLE, collisionRadius, 13200, 11200, sightRange, 0xff26ac0f)); //bot

            _game.ObjectManager.AddObject(new Nexus(_game, "OrderNexus", TeamId.TEAM_BLUE, collisionRadius, 1170, 1470, sightRange, 0xfff97db5));
            _game.ObjectManager.AddObject(new Nexus(_game, "ChaosNexus", TeamId.TEAM_PURPLE, collisionRadius, 12800, 13100, sightRange, 0xfff02c0f));
        }

        public void Update(float diff)
        {
            if (_game.GameTime >= 120 * 1000)
            {
                IsKillGoldRewardReductionActive = false;
            }

            if (SpawnEnabled)
            {
                if (_minionNumber > 0)
                {
                    if (_game.GameTime >= _nextSpawnTime + _minionNumber * 8 * 100)
                    { // Spawn new wave every 0.8s
                        if (Spawn())
                        {
                            _minionNumber = 0;
                            _nextSpawnTime += _spawnInterval;
                        }
                        else
                        {
                            _minionNumber++;
                        }
                    }
                }
                else if (_game.GameTime >= _nextSpawnTime)
                {
                    Spawn();
                    _minionNumber++;
                }
            }

            foreach (var fountain in _fountains.Values)
            {
                fountain.Update(diff);
            }
        }

        public ITarget GetRespawnLocation(TeamId team)
        {
            if (!SpawnsByTeam.ContainsKey(team))
            {
                return new Target(25.90f, 280);
            }

            return SpawnsByTeam[team];
        }

        public string GetMinionModel(TeamId team, MinionSpawnType type)
        {
            var teamDictionary = new Dictionary<TeamId, string>
            {
                {TeamId.TEAM_BLUE, "Blue"},
                {TeamId.TEAM_PURPLE, "Red"}
            };

            var typeDictionary = new Dictionary<MinionSpawnType, string>
            {
                {MinionSpawnType.MINION_TYPE_MELEE, "Basic"},
                {MinionSpawnType.MINION_TYPE_CASTER, "Wizard"},
                {MinionSpawnType.MINION_TYPE_CANNON, "MechCannon"},
                {MinionSpawnType.MINION_TYPE_SUPER, "MechMelee"}
            };

            if (!teamDictionary.ContainsKey(team) || !typeDictionary.ContainsKey(type))
            {
                return string.Empty;
            }

            return $"{teamDictionary[team]}_Minion_{typeDictionary[type]}";
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
                { MinionSpawnType.MINION_TYPE_MELEE, 19.8f + 0.2f * (int)(_game.GameTime / (90 * 1000)) },
                { MinionSpawnType.MINION_TYPE_CASTER, 16.8f + 0.2f * (int)(_game.GameTime / (90 * 1000)) },
                { MinionSpawnType.MINION_TYPE_CANNON, 40.0f + 0.5f * (int)(_game.GameTime / (90 * 1000)) },
                { MinionSpawnType.MINION_TYPE_SUPER, 40.0f + 1.0f * (int)(_game.GameTime / (180 * 1000)) }
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

        public Tuple<TeamId, Vector2> GetMinionSpawnPosition(MinionSpawnPosition spawnPosition)
        {
            switch (spawnPosition)
            {
                case MinionSpawnPosition.SPAWN_BLUE_TOP:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_BLUE, new Vector2(907, 1715));
                case MinionSpawnPosition.SPAWN_BLUE_BOT:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_BLUE, new Vector2(1533, 1321));
                case MinionSpawnPosition.SPAWN_BLUE_MID:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_BLUE, new Vector2(1443, 1663));
                case MinionSpawnPosition.SPAWN_RED_TOP:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_PURPLE, new Vector2(14455, 13159));
                case MinionSpawnPosition.SPAWN_RED_BOT:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_PURPLE, new Vector2(12967, 12695));
                case MinionSpawnPosition.SPAWN_RED_MID:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_PURPLE, new Vector2(12433, 12623));
            }
            return new Tuple<TeamId, Vector2>(0, new Vector2());
        }

        public void SetMinionStats(ILaneMinion m)
        {
            // Same for all minions
            m.Stats.MoveSpeed.BaseValue = 325.0f;

            switch (m.MinionSpawnType)
            {
                case MinionSpawnType.MINION_TYPE_MELEE:
                    m.Stats.CurrentHealth = 475.0f + 20.0f * (int)(_game.GameTime / (180 * 1000));
                    m.Stats.HealthPoints.BaseValue = 475.0f + 20.0f * (int)(_game.GameTime / (180 * 1000));
                    m.Stats.AttackDamage.BaseValue = 12.0f + 1.0f * (int)(_game.GameTime / (180 * 1000));
                    m.Stats.Range.BaseValue = 180.0f;
                    m.Stats.AttackSpeedFlat = 1.250f;
                    m.AutoAttackDelay = 11.8f / 30.0f;
                    m.IsMelee = true;
                    break;
                case MinionSpawnType.MINION_TYPE_CASTER:
                    m.Stats.CurrentHealth = 279.0f + 7.5f * (int)(_game.GameTime / (90 * 1000));
                    m.Stats.HealthPoints.BaseValue = 279.0f + 7.5f * (int)(_game.GameTime / (90 * 1000));
                    m.Stats.AttackDamage.BaseValue = 23.0f + 1.0f * (int)(_game.GameTime / (90 * 1000));
                    m.Stats.Range.BaseValue = 600.0f;
                    m.Stats.AttackSpeedFlat = 0.670f;
                    m.AutoAttackDelay = 14.1f / 30.0f;
                    m.AutoAttackProjectileSpeed = 650.0f;
                    break;
                case MinionSpawnType.MINION_TYPE_CANNON:
                    m.Stats.CurrentHealth = 700.0f + 27.0f * (int)(_game.GameTime / (180 * 1000));
                    m.Stats.HealthPoints.BaseValue = 700.0f + 27.0f * (int)(_game.GameTime / (180 * 1000));
                    m.Stats.AttackDamage.BaseValue = 40.0f + 3.0f * (int)(_game.GameTime / (180 * 1000));
                    m.Stats.Range.BaseValue = 450.0f;
                    m.Stats.AttackSpeedFlat = 1.0f;
                    m.AutoAttackDelay = 9.0f / 30.0f;
                    m.AutoAttackProjectileSpeed = 1200.0f;
                    break;
                case MinionSpawnType.MINION_TYPE_SUPER:
                    m.Stats.CurrentHealth = 1500.0f + 200.0f * (int)(_game.GameTime / (180 * 1000));
                    m.Stats.HealthPoints.BaseValue = 1500.0f + 200.0f * (int)(_game.GameTime / (180 * 1000));
                    m.Stats.AttackDamage.BaseValue = 190.0f + 10.0f * (int)(_game.GameTime / (180 * 1000));
                    m.Stats.Range.BaseValue = 170.0f;
                    m.Stats.AttackSpeedFlat = 0.694f;
                    m.Stats.Armor.BaseValue = 30.0f;
                    m.Stats.MagicResist.BaseValue = -30.0f;
                    m.IsMelee = true;
                    m.AutoAttackDelay = 15.0f / 30.0f;
                    break;
            }
        }

        public void SpawnMinion(List<MinionSpawnType> list, int minionNo, MinionSpawnPosition pos, List<Vector2> waypoints)
        {
            if (list.Count <= minionNo)
            {
                return;
            }

            var m = new LaneMinion(_game, list[minionNo], pos, waypoints);
            _game.ObjectManager.AddObject(m);
        }

        public bool Spawn()
        {
            var positions = new List<MinionSpawnPosition>
            {
                MinionSpawnPosition.SPAWN_BLUE_TOP,
                MinionSpawnPosition.SPAWN_BLUE_BOT,
                MinionSpawnPosition.SPAWN_BLUE_MID,
                MinionSpawnPosition.SPAWN_RED_TOP,
                MinionSpawnPosition.SPAWN_RED_BOT,
                MinionSpawnPosition.SPAWN_RED_MID
            };

            var cannonMinionTimestamps = new List<Tuple<long, int>>
            {
                new Tuple<long, int>(0, 2),
                new Tuple<long, int>(20 * 60 * 1000, 1),
                new Tuple<long, int>(35 * 60 * 1000, 0)
            };

            var spawnToWaypoints = new Dictionary<MinionSpawnPosition, Tuple<List<Vector2>, uint>>
            {
                {MinionSpawnPosition.SPAWN_BLUE_BOT, Tuple.Create(BlueBotWaypoints, 0xff26ac0f)},
                {MinionSpawnPosition.SPAWN_BLUE_MID, Tuple.Create(BlueMidWaypoints, 0xffff8f1f)},
                {MinionSpawnPosition.SPAWN_BLUE_TOP, Tuple.Create(BlueTopWaypoints, 0xff6793d0)},
                {MinionSpawnPosition.SPAWN_RED_BOT, Tuple.Create(RedBotWaypoints, 0xff9303e1)},
                {MinionSpawnPosition.SPAWN_RED_MID, Tuple.Create(RedMidWaypoints, 0xff4a20f1)},
                {MinionSpawnPosition.SPAWN_RED_TOP, Tuple.Create(RedTopWaypoints, 0xffd23c3e)}
            };
            var cannonMinionCap = 2;

            foreach (var timestamp in cannonMinionTimestamps)
            {
                if (_game.GameTime >= timestamp.Item1)
                {
                    cannonMinionCap = timestamp.Item2;
                }
            }

            foreach (var pos in positions)
            {
                var waypoints = spawnToWaypoints[pos].Item1;
                var inhibitorId = spawnToWaypoints[pos].Item2;
                var inhibitor = _game.ObjectManager.GetInhibitorById(inhibitorId);
                var isInhibitorDead = inhibitor.InhibitorState == InhibitorState.DEAD && !inhibitor.RespawnAnnounced;

                var oppositeTeam = TeamId.TEAM_BLUE;
                if (inhibitor.Team == TeamId.TEAM_PURPLE)
                {
                    oppositeTeam = TeamId.TEAM_PURPLE;
                }

                var areAllInhibitorsDead = _game.ObjectManager.AllInhibitorsDestroyedFromTeam(oppositeTeam) && !inhibitor.RespawnAnnounced;

                var list = RegularMinionWave;
                if (_cannonMinionCount >= cannonMinionCap)
                {
                    list = CannonMinionWave;
                }

                if (isInhibitorDead)
                {
                    list = SuperMinionWave;
                }

                if (areAllInhibitorsDead)
                {
                    list = DoubleSuperMinionWave;
                }

                SpawnMinion(list, _minionNumber, pos, waypoints);
            }


            if (_minionNumber < 8)
            {
                return false;
            }

            if (_cannonMinionCount >= cannonMinionCap)
            {
                _cannonMinionCount = 0;
            }
            else
            {
                _cannonMinionCount++;
            }
            return true;
        }

        public Vector3 GetEndGameCameraPosition(TeamId team)
        {
            if (!EndGameCameraPosition.ContainsKey(team))
            {
                return new Vector3(0, 0, 0);
            }

            return EndGameCameraPosition[team];
        }
    }
}
