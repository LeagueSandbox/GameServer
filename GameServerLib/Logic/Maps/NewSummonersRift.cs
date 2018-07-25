using System;
using System.Collections.Generic;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.Logic.GameObjects.Other;
using LeagueSandbox.GameServer.Logic.Scripting.CSharp;

namespace LeagueSandbox.GameServer.Logic.Maps
{
    internal class NewSummonersRift : IMapGameScript
    {
        private static readonly List<Vector2> BlueTopWaypoints = new List<Vector2>
        {
            new Vector2(1118.989f, 2082.105f),
            new Vector2(1475.725f, 4284.732f),
            new Vector2(1170.178f, 6635.276f),
            new Vector2(1254.696f, 10362.6f),
            new Vector2(1433.61f, 11615.48f),
            new Vector2(2102.231f, 12525.56f),
            new Vector2(3113.075f, 13305.71f),
            new Vector2(4230.007f, 13464.23f),
            new Vector2(7663.801f, 13627.43f),
            new Vector2(10466.06f, 13351.26f),
            new Vector2(12790.75f, 13748.61f)
        };
        private static readonly List<Vector2> BlueBotWaypoints = new List<Vector2>
        {
            new Vector2(2054.545f, 1174.528f),
            new Vector2(3436.437f, 947.2966f),
            new Vector2(6918.244f, 1189.622f),
            new Vector2(10481.45f, 1283.461f),
            new Vector2(11593.5f, 1479.206f),
            new Vector2(12745.49f, 2477.221f),
            new Vector2(13317.6f, 3275.311f),
            new Vector2(13552.65f, 4492.224f),
            new Vector2(13623.62f, 8220.113f),
            new Vector2(13872.56f, 11328.7f),
            new Vector2(13720.09f, 12853.67f)
        };
        private static readonly List<Vector2> BlueMidWaypoints = new List<Vector2>
        {
            new Vector2(2041.072f, 2068.447f),
            new Vector2(3402.388f, 2980.479f),
            new Vector2(4834.421f, 4996.653f),
            new Vector2(6053.542f, 6133.397f),
            new Vector2(7475.346f, 7391.124f),
            new Vector2(8734.588f, 8665.224f),
            new Vector2(10008.18f, 9911.293f),
            new Vector2(11373.97f, 11887.83f),
            new Vector2(12751.62f, 12799.5f)
        };
        private static readonly List<Vector2> RedTopWaypoints = new List<Vector2>
        {
            new Vector2(12790.75f, 13748.61f),
            new Vector2(10466.06f, 13351.26f),
            new Vector2(7663.801f, 13627.43f),
            new Vector2(4230.007f, 13464.23f),
            new Vector2(3113.075f, 13305.71f),
            new Vector2(2102.231f, 12525.56f),
            new Vector2(1433.61f, 11615.48f),
            new Vector2(1254.696f, 10362.6f),
            new Vector2(1170.178f, 6635.276f),
            new Vector2(1475.725f, 4284.732f),
            new Vector2(1118.989f, 2082.105f)
        };
        private static readonly List<Vector2> RedBotWaypoints = new List<Vector2>
        {
            new Vector2(13720.09f, 12853.67f),
            new Vector2(13872.56f, 11328.7f),
            new Vector2(13623.62f, 8220.113f),
            new Vector2(13552.65f, 4492.224f),
            new Vector2(13317.6f, 3275.311f),
            new Vector2(12745.49f, 2477.221f),
            new Vector2(11593.5f, 1479.206f),
            new Vector2(10481.45f, 1283.461f),
            new Vector2(6918.244f, 1189.622f),
            new Vector2(3436.437f, 947.2966f),
            new Vector2(2054.545f, 1174.528f)
        };
        private static readonly List<Vector2> RedMidWaypoints = new List<Vector2>
        {
            new Vector2(12751.62f, 12799.5f),
            new Vector2(11373.97f, 11887.83f),
            new Vector2(10008.18f, 9911.293f),
            new Vector2(8734.588f, 8665.224f),
            new Vector2(7475.346f, 7391.124f),
            new Vector2(6053.542f, 6133.397f),
            new Vector2(4834.421f, 4996.653f),
            new Vector2(3402.388f, 2980.479f),
            new Vector2(2041.072f, 2068.447f)
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
            { TeamId.TEAM_BLUE, new Vector3(1549.357f, 1641.553f, 188) },
            { TeamId.TEAM_PURPLE, new Vector3(13231.74f, 13269.85f, 110) }
        };

        private static readonly Dictionary<TeamId, Target> SpawnsByTeam = new Dictionary<TeamId, Target>
        {
            {TeamId.TEAM_BLUE, new Target(411.3218f, 397.2352f)},
            {TeamId.TEAM_PURPLE, new Target(14301.51f, 14367.99f)}
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
        public bool HasFirstBloodHappened { get; set; } = false;
        public bool IsKillGoldRewardReductionActive { get; set; } = true;
        public int BluePillId { get; set; } = 2001;
        public long FirstGoldTime { get; set; } = 90 * 1000;
        public bool SpawnEnabled { get; set; }
        public NewSummonersRift(Game game)
        {
            _game = game;
            _fountains = new Dictionary<TeamId, Fountain>
            {
                { TeamId.TEAM_BLUE, new Fountain(game, TeamId.TEAM_BLUE, 1549.357f, 1641.553f, 1000) },
                { TeamId.TEAM_PURPLE, new Fountain(game, TeamId.TEAM_PURPLE, 14301.51f, 14367.99f, 1000) }
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

            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_R_03_A", 10493.31f, 1032.618f, TeamId.TEAM_BLUE,
                TurretType.OUTER_TURRET, GetTurretItems(TurretType.OUTER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_R_02_A", 6920.977f, 1502.196f, TeamId.TEAM_BLUE,
                TurretType.INNER_TURRET, GetTurretItems(TurretType.INNER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_C_07_A", 4289.411f, 1284.22f, TeamId.TEAM_BLUE,
                TurretType.INHIBITOR_TURRET, GetTurretItems(TurretType.INHIBITOR_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_R_03_A", 13868.79f, 4514.624f, TeamId.TEAM_PURPLE,
                TurretType.OUTER_TURRET, GetTurretItems(TurretType.OUTER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_R_02_A", 13323.85f, 8231.193f, TeamId.TEAM_PURPLE,
                TurretType.INNER_TURRET, GetTurretItems(TurretType.INNER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_R_01_A", 13625.38f, 10571.88f, TeamId.TEAM_PURPLE,
                TurretType.INHIBITOR_TURRET, GetTurretItems(TurretType.INHIBITOR_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_C_05_A", 5844.089f, 6390.292f, TeamId.TEAM_BLUE,
                TurretType.OUTER_TURRET, GetTurretItems(TurretType.OUTER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_C_04_A", 5040.118f, 4801.884f, TeamId.TEAM_BLUE,
                TurretType.INNER_TURRET, GetTurretItems(TurretType.INNER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_C_03_A", 3653.022f, 3709.27f, TeamId.TEAM_BLUE,
                TurretType.INHIBITOR_TURRET, GetTurretItems(TurretType.INHIBITOR_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_C_01_A", 1739.356f, 2266.655f, TeamId.TEAM_BLUE,
                TurretType.NEXUS_TURRET, GetTurretItems(TurretType.NEXUS_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_C_02_A", 2169.351f, 1807.156f, TeamId.TEAM_BLUE,
                TurretType.NEXUS_TURRET, GetTurretItems(TurretType.NEXUS_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_C_05_A", 8954.321f, 8509.283f, TeamId.TEAM_PURPLE,
                TurretType.OUTER_TURRET, GetTurretItems(TurretType.OUTER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_C_04_A", 9768.862f, 10105.47f, TeamId.TEAM_PURPLE,
                TurretType.INNER_TURRET, GetTurretItems(TurretType.INNER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_C_03_A", 11127.94f, 11218.85f, TeamId.TEAM_PURPLE,
                TurretType.INHIBITOR_TURRET, GetTurretItems(TurretType.INHIBITOR_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_C_01_A", 13052.38f, 12618.45f, TeamId.TEAM_PURPLE,
                TurretType.NEXUS_TURRET, GetTurretItems(TurretType.NEXUS_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_C_02_A", 12611.25f, 13082.25f, TeamId.TEAM_PURPLE,
                TurretType.NEXUS_TURRET, GetTurretItems(TurretType.NEXUS_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_OrderTurretShrine_A", 101.8922f, 81.27875f, TeamId.TEAM_BLUE,
                TurretType.FOUNTAIN_TURRET, GetTurretItems(TurretType.FOUNTAIN_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_ChaosTurretShrine_A", 14578.52f, 14655f, TeamId.TEAM_PURPLE,
                TurretType.FOUNTAIN_TURRET, GetTurretItems(TurretType.FOUNTAIN_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_L_03_A", 980.1478f, 10415.1f, TeamId.TEAM_BLUE,
                TurretType.OUTER_TURRET, TurretItems[TurretType.OUTER_TURRET]));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_L_02_A", 1509.917f, 6671.09f, TeamId.TEAM_BLUE,
                TurretType.INNER_TURRET, GetTurretItems(TurretType.INNER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T1_C_06_A", 1168.235f, 4259.218f, TeamId.TEAM_BLUE,
                TurretType.INHIBITOR_TURRET, GetTurretItems(TurretType.INHIBITOR_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_L_03_A", 4323.455f, 13865.75f, TeamId.TEAM_PURPLE,
                TurretType.OUTER_TURRET, TurretItems[TurretType.OUTER_TURRET]));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_L_02_A", 7948.803f, 13375.36f, TeamId.TEAM_PURPLE,
                TurretType.INNER_TURRET, GetTurretItems(TurretType.INNER_TURRET)));
            _game.ObjectManager.AddObject(new LaneTurret(_game, "Turret_T2_L_01_A", 10477.9f, 13655.53f, TeamId.TEAM_PURPLE,
                TurretType.INHIBITOR_TURRET, GetTurretItems(TurretType.INHIBITOR_TURRET)));

            _game.ObjectManager.AddObject(new LevelProp(_game, 13166.86f, 14174f, 101.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, "LevelProp_Yonkey", "Yonkey"));
            _game.ObjectManager.AddObject(new LevelProp(_game, 588.7625f, 1634.94f, 94.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, "LevelProp_Yonkey1", "Yonkey"));
            _game.ObjectManager.AddObject(new LevelProp(_game, 13668.32f, 14591.46f, 194.9741f, 224.0f, 33.33f, 0.0f, 0.0f, -44.44f, "LevelProp_ShopMale", "ShopMale"));
            _game.ObjectManager.AddObject(new LevelProp(_game, 114.2483f, 1041.24f, 191.4039f, 158.0f, 0.0f, 0.0f, 0.0f, 0.0f, "LevelProp_ShopMale1", "ShopMale"));

            //TODO
            var collisionRadius = 0;
            var sightRange = 1700;

            _game.ObjectManager.AddObject(new Inhibitor(_game, "OrderInhibitor", TeamId.TEAM_BLUE, collisionRadius, 1179.751f, 3584.724f, sightRange, 0xffd23c3e)); //top
            _game.ObjectManager.AddObject(new Inhibitor(_game, "OrderInhibitor", TeamId.TEAM_BLUE, collisionRadius, 3204.638f, 3241.247f, sightRange, 0xff4a20f1)); //mid
            _game.ObjectManager.AddObject(new Inhibitor(_game, "OrderInhibitor", TeamId.TEAM_BLUE, collisionRadius, 3478.902f, 1250.106f, sightRange, 0xff9303e1)); //bot
            _game.ObjectManager.AddObject(new Inhibitor(_game, "ChaosInhibitor", TeamId.TEAM_PURPLE, collisionRadius, 11260.25f, 13680.02f, sightRange, 0xff6793d0)); //top
            _game.ObjectManager.AddObject(new Inhibitor(_game, "ChaosInhibitor", TeamId.TEAM_PURPLE, collisionRadius, 11591.28f, 11667.36f, sightRange, 0xffff8f1f)); //mid
            _game.ObjectManager.AddObject(new Inhibitor(_game, "ChaosInhibitor", TeamId.TEAM_PURPLE, collisionRadius, 13614.68f, 11309.92f, sightRange, 0xff26ac0f)); //bot

            _game.ObjectManager.AddObject(new Nexus(_game, "OrderNexus", TeamId.TEAM_BLUE, collisionRadius, 1549.357f, 1641.553f, sightRange, 0xfff97db5));
            _game.ObjectManager.AddObject(new Nexus(_game, "ChaosNexus", TeamId.TEAM_PURPLE, collisionRadius, 13231.74f, 13269.85f, sightRange, 0xfff02c0f));
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

        public Target GetRespawnLocation(TeamId team)
        {
            if (!SpawnsByTeam.ContainsKey(team))
            {
                return new Target(1549.357f, 1641.553f);
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

        public float GetGoldFor(AttackableUnit u)
        {
            if (!(u is Minion m))
            {
                if (!(u is Champion c))
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

                if (c.KillDeathCounter < 0)
                {
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

                return 0.0f;
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

        public float GetExperienceFor(AttackableUnit u)
        {
            if (!(u is Minion m))
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
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_BLUE, new Vector2(1118.989f, 2082.105f));
                case MinionSpawnPosition.SPAWN_BLUE_BOT:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_BLUE, new Vector2(2054.545f, 1174.528f));
                case MinionSpawnPosition.SPAWN_BLUE_MID:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_BLUE, new Vector2(2041.072f, 2068.447f));
                case MinionSpawnPosition.SPAWN_RED_TOP:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_PURPLE, new Vector2(12790.75f, 13748.61f));
                case MinionSpawnPosition.SPAWN_RED_BOT:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_PURPLE, new Vector2(13720.09f, 12853.67f));
                case MinionSpawnPosition.SPAWN_RED_MID:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_PURPLE, new Vector2(12751.62f, 12799.5f));
            }
            return new Tuple<TeamId, Vector2>(0, new Vector2());
        }

        public void SetMinionStats(Minion m)
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

            var m = new Minion(_game, list[minionNo], pos, waypoints);
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
