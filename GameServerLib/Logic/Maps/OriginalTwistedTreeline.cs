using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;


namespace LeagueSandbox.GameServer.Logic.Maps
{
    class OriginalTwistedTreeline : Map
    {
        private Logger _logger = Program.ResolveDependency<Logger>();
        private int _cannonMinionCount;

        private static readonly List<Vector2> BLUE_TOP_WAYPOINTS = new List<Vector2>
        {
            new Vector2(1960f, 6684f),
            new Vector2(1410.101f, 8112.747f),
            new Vector2(2153f, 9228.292f),
            new Vector2(3254.654f, 8801.554f),
            new Vector2(3967.261f, 7689.167f),
            new Vector2(5187.636f, 7075.806f),
            new Vector2(6742.38f, 6797.492f),
            new Vector2(8587.43f, 7340.201f),
            new Vector2(9798.584f, 8079.067f),
            new Vector2(10926.82f, 9423.166f),
            new Vector2(12042.19f, 8413.8f),
            new Vector2(11758.64f, 6586.07f)
        };

        private static readonly List<Vector2> BLUE_BOT_WAYPOINTS = new List<Vector2>
        {
            new Vector2(1960f, 5977f),
            new Vector2(1340.854f, 4566.956f),
            new Vector2(2218.08f, 3771.371f),
            new Vector2(3822.184f, 3735.409f),
            new Vector2(5268.443f, 3410.666f),
            new Vector2(6766.503f, 3435.633f),
            new Vector2(8776.482f, 34230.031f),
            new Vector2(10120.18f, 3735.568f),
            new Vector2(11622.86f, 3707.629f),
            new Vector2(12086.33f, 4369.503f),
            new Vector2(11797.77f, 6077.822f)
        };

        private static readonly List<Vector2> RED_TOP_WAYPOINTS = new List<Vector2>
        {
            new Vector2(11420f, 6617f),
            new Vector2(11980.1f, 7735.383f),
            new Vector2(12017.56f, 8453.665f),
            new Vector2(10831.11f, 9381.894f),
            new Vector2(9815.62f, 8208.034f),
            new Vector2(9815.62f, 8208.034f),
            new Vector2(8462.342f, 7282.162f),
            new Vector2(8462.342f, 7282.162f),
            new Vector2(6748.594f, 6805.058f),
            new Vector2(5014.874f, 7184.514f),
            new Vector2(3769.57f, 7920.32f),
            new Vector2(2894.403f, 9219.018f),
            new Vector2(1467.724f, 8491.606f),
            new Vector2(1498.947f, 7408.595f),
            new Vector2(1636.39f, 6486.415f)
        };

        private static readonly List<Vector2> RED_BOT_WAYPOINTS = new List<Vector2>
        {
            new Vector2(11420f, 6024f),
            new Vector2(11999.55f, 5145.43f),
            new Vector2(12037.32f, 4089.385f),
            new Vector2(10113.69f, 3693.272f),
            new Vector2(8829.546f, 3412.617f),
            new Vector2(6729.558f, 3454.635f),
            new Vector2(4695.755f, 3401.182f),
            new Vector2(3374.252f, 3714.507f),
            new Vector2(1384.365f, 4558.863f),
            new Vector2(1582.978f, 6066.552f)
        };

        private static readonly List<MinionSpawnType> REGULAR_MINION_WAVE = new List<MinionSpawnType>
        {
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER
        };
        private static readonly List<MinionSpawnType> CANNON_MINION_WAVE = new List<MinionSpawnType>
        {
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CANNON,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER
        };
        private static readonly List<MinionSpawnType> SUPER_MINION_WAVE = new List<MinionSpawnType>
        {
            MinionSpawnType.MINION_TYPE_SUPER,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER
        };

        private Dictionary<TeamId, float[]> _endGameCameraPosition = new Dictionary<TeamId, float[]>
        {
            { TeamId.TEAM_BLUE, new[] { 2136.014f, 6311.61035f, 188 } },
            { TeamId.TEAM_PURPLE, new[] { 11199.9639f, 6312.051f, 110 } }
        };

        private static readonly Dictionary<TurretType, int[]> _turretItems = new Dictionary<TurretType, int[]>
        {
            { TurretType.OuterTurret, new[] { 1500, 1501, 1502, 1503 } },
            { TurretType.InnerTurret, new[] { 1500, 1501, 1502, 1503, 1504 } },
            { TurretType.InhibitorTurret, new[] { 1501, 1502, 1503, 1505 } },
            { TurretType.NexusTurret, new[] { 1501, 1502, 1503, 1505 } }
        };

        private static readonly Dictionary<MinionSpawnType, float> _xpForMinion = new Dictionary<MinionSpawnType, float>
        {
            { MinionSpawnType.MINION_TYPE_MELEE, 58.88f },
            { MinionSpawnType.MINION_TYPE_CASTER, 29.44f },
            { MinionSpawnType.MINION_TYPE_CANNON, 92.0f },
            { MinionSpawnType.MINION_TYPE_SUPER, 97.0f }
        };

        public OriginalTwistedTreeline(Game game) : base(game, 90 * 1000, 30 * 1000, 90 * 1000, true, 4)
        {
            var path = System.IO.Path.Combine(
                Program.ExecutingDirectory,
                "Content",
                "Data",
                _game.Config.ContentManager.GameModeName,
                "AIMesh",
                "Map" + GetMapId(),
                "AIPath.aimesh"
            );

            if (File.Exists(path))
            {
                AIMesh = new RAF.AIMesh(path);
            }
            else
            {
                _logger.LogCoreError("Failed to load Original Twisted Treeline data.");
                return;
            }

            AddObject(new LaneTurret("Turret_OrderTurretShrine_A", -369.365967f, 6362.34766f, TeamId.TEAM_BLUE, TurretType.FountainTurret));
            AddObject(new LaneTurret("Turret_T1_C_01_A", 1552.661f, 6320.81641f, TeamId.TEAM_BLUE, TurretType.NexusTurret, GetTurretItems(TurretType.NexusTurret)));
            AddObject(new LaneTurret("Turret_T1_C_06_A", 1139.43726f, 8629.616f, TeamId.TEAM_BLUE, TurretType.InhibitorTurret, GetTurretItems(TurretType.InhibitorTurret)));
            AddObject(new LaneTurret("Turret_T1_C_07_A", 1095.27039f, 3984.54346f, TeamId.TEAM_BLUE, TurretType.InhibitorTurret, GetTurretItems(TurretType.InhibitorTurret)));
            AddObject(new LaneTurret("Turret_T1_L_02_A", 3946.62427f, 8156.286f, TeamId.TEAM_BLUE, TurretType.InnerTurret, GetTurretItems(TurretType.InnerTurret)));
            AddObject(new LaneTurret("Turret_T1_R_02_A", 3314.54126f, 3375.20752f, TeamId.TEAM_BLUE, TurretType.InnerTurret, GetTurretItems(TurretType.InnerTurret)));

            AddObject(new LaneTurret("Turret_ChaosTurretShrine_A", 13740.7f, 6395.247f, TeamId.TEAM_PURPLE, TurretType.FountainTurret));
            AddObject(new LaneTurret("Turret_T2_C_01_A", 11750.8838f, 6288.757f, TeamId.TEAM_PURPLE, TurretType.NexusTurret, GetTurretItems(TurretType.NexusTurret)));
            AddObject(new LaneTurret("Turret_T2_L_01_A", 12332.5508f, 8572.488f, TeamId.TEAM_PURPLE, TurretType.InhibitorTurret, GetTurretItems(TurretType.InhibitorTurret)));
            AddObject(new LaneTurret("Turret_T2_R_01_A", 12355.7578f, 4041.023f, TeamId.TEAM_PURPLE, TurretType.InhibitorTurret, GetTurretItems(TurretType.InhibitorTurret)));
            AddObject(new LaneTurret("Turret_T2_R_02_A", 10079.3438f, 3400.3772f, TeamId.TEAM_PURPLE, TurretType.InnerTurret, GetTurretItems(TurretType.InnerTurret)));
            AddObject(new LaneTurret("Turret_T2_L_02_A", 9562.389f, 8235.934f, TeamId.TEAM_PURPLE, TurretType.InnerTurret, GetTurretItems(TurretType.InnerTurret)));



            //TODO
            var COLLISION_RADIUS = 0;
            var SIGHT_RANGE = 1700;

            AddObject(new Inhibitor("OrderInhibitor", TeamId.TEAM_BLUE, COLLISION_RADIUS, 895.594f, 8153.52832f, SIGHT_RANGE, 0xffd23c3e)); //top
            AddObject(new Inhibitor("OrderInhibitor", TeamId.TEAM_BLUE, COLLISION_RADIUS, 841.4701f, 4564.76074f, SIGHT_RANGE, 0xff9303e1)); //bot
            AddObject(new Inhibitor("ChaosInhibitor", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 12563.9375f, 8092.63574f, SIGHT_RANGE, 0xff6793d0)); //top
            AddObject(new Inhibitor("ChaosInhibitor", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 12706.2969f, 4483.154f, SIGHT_RANGE, 0xff26ac0f)); //bot

            AddObject(new Nexus("OrderNexus", TeamId.TEAM_BLUE, COLLISION_RADIUS, 2136.014f, 6311.61035f, SIGHT_RANGE, 0xfff97db5));
            AddObject(new Nexus("ChaosNexus", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 11199.9639f, 6312.051f, SIGHT_RANGE, 0xfff02c0f));

            // Start at xp to reach level 1
            ExpToLevelUp = new List<int> { 0, 280, 660, 1140, 1720, 2400, 3180, 4060, 5040, 6120, 7300, 8580, 9960, 11440, 13020, 14700, 16480, 18360 };

            // Announcer events
            _announcerEvents.Add(new Announce(game, 30 * 1000, Announces.WelcomeToSR, true)); // Welcome to SR
            if (_firstSpawnTime - 30 * 1000 >= 0.0f)
                _announcerEvents.Add(new Announce(game, _firstSpawnTime - 30 * 1000, Announces.ThirySecondsToMinionsSpawn, true)); // 30 seconds until minions spawn
            _announcerEvents.Add(new Announce(game, _firstSpawnTime, Announces.MinionsHaveSpawned, false)); // Minions have spawned (90 * 1000)
            _announcerEvents.Add(new Announce(game, _firstSpawnTime, Announces.MinionsHaveSpawned2, false)); // Minions have spawned [2] (90 * 1000)
        }

        public int[] GetTurretItems(TurretType type)
        {
            if (!_turretItems.ContainsKey(type))
                return null;

            return _turretItems[type];
        }

        public override void Update(float diff)
        {
            base.Update(diff);

            if (GameTime >= 120 * 1000)
            {
                IsKillGoldRewardReductionActive = false;
            }
        }
        public override float GetGoldPerSecond()
        {
            return 1.9f;
        }

        public override Target GetRespawnLocation(TeamId team)
        {
            var dic = new Dictionary<TeamId, Target>
            {
                { TeamId.TEAM_BLUE, new Target(23f, 6371f) },
                { TeamId.TEAM_PURPLE, new Target(13384f, 6378f) }
            };

            if (!dic.ContainsKey(team))
            {
                return dic[0];
            }

            return dic[team];
        }

        public override float GetGoldFor(Unit u)
        {
            var m = u as Minion;
            if (m == null)
            {
                var c = u as Champion;
                if (c == null)
                    return 0.0f;

                float gold = 300.0f; //normal gold for a kill
                if (c.KillDeathCounter < 5 && c.KillDeathCounter >= 0)
                {
                    if (c.KillDeathCounter == 0)
                        return gold;
                    for (int i = c.KillDeathCounter; i > 1; --i)
                        gold += gold * 0.165f;

                    return gold;
                }

                if (c.KillDeathCounter >= 5)
                    return 500.0f;

                if (c.KillDeathCounter < 0)
                {
                    var firstDeathGold = gold - gold * 0.085f;

                    if (c.KillDeathCounter == -1)
                        return firstDeathGold;

                    for (int i = c.KillDeathCounter; i < -1; ++i)
                        firstDeathGold -= firstDeathGold * 0.2f;

                    if (firstDeathGold < 50)
                        firstDeathGold = 50;

                    return firstDeathGold;
                }

                return 0.0f;
            }

            var _goldForMinion = new Dictionary<MinionSpawnType, float>
            {
                { MinionSpawnType.MINION_TYPE_MELEE, 19.0f + 0.5f * (int)(GameTime / (180 * 1000)) },
                { MinionSpawnType.MINION_TYPE_CASTER, 14.0f + 0.2f * (int)(GameTime / (90 * 1000)) },
                { MinionSpawnType.MINION_TYPE_CANNON, 40.0f + 1.0f * (int)(GameTime / (180 * 1000)) },
                { MinionSpawnType.MINION_TYPE_SUPER, 40.0f + 1.0f * (int)(GameTime / (180 * 1000)) }
            };
            return _goldForMinion[m.getType()];
        }
        public override float GetExperienceFor(Unit u)
        {
            var m = u as Minion;

            if (m == null)
                return 0.0f;

            return _xpForMinion[m.getType()];
        }

        public override Tuple<TeamId, Vector2> GetMinionSpawnPosition(MinionSpawnPosition spawnPosition)
        {
            switch (spawnPosition)
            {
                case MinionSpawnPosition.SPAWN_BLUE_TOP:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_BLUE, new Vector2(1960, 6684));
                case MinionSpawnPosition.SPAWN_BLUE_BOT:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_BLUE, new Vector2(1960, 5977));
                case MinionSpawnPosition.SPAWN_RED_TOP:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_PURPLE, new Vector2(11420, 6617));
                case MinionSpawnPosition.SPAWN_RED_BOT:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_PURPLE, new Vector2(11420, 6024));
            }
            return new Tuple<TeamId, Vector2>(0, new Vector2());
        }
        public override void SetMinionStats(Minion m)
        {
            // Same for all minions
            m.GetStats().MoveSpeed.BaseValue = 325.0f;

            switch (m.getType())
            {
                case MinionSpawnType.MINION_TYPE_MELEE:
                    m.GetStats().CurrentHealth = 475.0f + 20.0f * (int)(GameTime / (float)(180 * 1000));
                    m.GetStats().HealthPoints.BaseValue = 475.0f + 20.0f * (int)(GameTime / (float)(180 * 1000));
                    m.GetStats().AttackDamage.BaseValue = 12.0f + 1.0f * (int)(GameTime / (float)(180 * 1000));
                    m.GetStats().Range.BaseValue = 180.0f;
                    m.GetStats().AttackSpeedFlat = 1.250f;
                    m.AutoAttackDelay = 11.8f / 30.0f;
                    m.IsMelee = true;
                    break;
                case MinionSpawnType.MINION_TYPE_CASTER:
                    m.GetStats().CurrentHealth = 279.0f + 7.5f * (int)(GameTime / (float)(90 * 1000));
                    m.GetStats().HealthPoints.BaseValue = 279.0f + 7.5f * (int)(GameTime / (float)(90 * 1000));
                    m.GetStats().AttackDamage.BaseValue = 23.0f + 1.0f * (int)(GameTime / (float)(90 * 1000));
                    m.GetStats().Range.BaseValue = 600.0f;
                    m.GetStats().AttackSpeedFlat = 0.670f;
                    m.AutoAttackDelay = 14.1f / 30.0f;
                    m.AutoAttackProjectileSpeed = 650.0f;
                    break;
                case MinionSpawnType.MINION_TYPE_CANNON:
                    m.GetStats().CurrentHealth = 700.0f + 27.0f * (int)(GameTime / (float)(180 * 1000));
                    m.GetStats().HealthPoints.BaseValue = 700.0f + 27.0f * (int)(GameTime / (float)(180 * 1000));
                    m.GetStats().AttackDamage.BaseValue = 40.0f + 3.0f * (int)(GameTime / (float)(180 * 1000));
                    m.GetStats().Range.BaseValue = 450.0f;
                    m.GetStats().AttackSpeedFlat = 1.0f;
                    m.AutoAttackDelay = 9.0f / 30.0f;
                    m.AutoAttackProjectileSpeed = 1200.0f;
                    break;
                case MinionSpawnType.MINION_TYPE_SUPER:
                    m.GetStats().CurrentHealth = 1500.0f + 200.0f * (int)(GameTime / (float)(180 * 1000));
                    m.GetStats().HealthPoints.BaseValue = 1500.0f + 200.0f * (int)(GameTime / (float)(180 * 1000));
                    m.GetStats().AttackDamage.BaseValue = 190.0f + 10.0f * (int)(GameTime / (float)(180 * 1000));
                    m.GetStats().Range.BaseValue = 170.0f;
                    m.GetStats().AttackSpeedFlat = 0.694f;
                    m.GetStats().Armor.BaseValue = 30.0f;
                    m.GetStats().MagicResist.BaseValue = -30.0f;
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

            var m = new Minion(list[minionNo], pos, waypoints);
            AddObject(m);
        }

        public override bool Spawn()
        {
            var positions = new List<MinionSpawnPosition>
            {
                MinionSpawnPosition.SPAWN_BLUE_TOP,
                MinionSpawnPosition.SPAWN_BLUE_BOT,
                MinionSpawnPosition.SPAWN_RED_TOP,
                MinionSpawnPosition.SPAWN_RED_BOT,
            };

            var cannonMinionTimestamps = new List<Tuple<long, int>>
            {
                new Tuple<long, int>(0, 2),
                new Tuple<long, int>(20 * 60 * 1000, 1),
                new Tuple<long, int>(35 * 60 * 1000, 0)
            };

            var spawnToWaypoints = new Dictionary<MinionSpawnPosition, Tuple<List<Vector2>, uint>>
            {
                { MinionSpawnPosition.SPAWN_BLUE_BOT, Tuple.Create(BLUE_BOT_WAYPOINTS, 0xff26ac0f) },
                { MinionSpawnPosition.SPAWN_BLUE_TOP, Tuple.Create(BLUE_TOP_WAYPOINTS, 0xff6793d0) },
                { MinionSpawnPosition.SPAWN_RED_BOT, Tuple.Create(RED_BOT_WAYPOINTS, 0xff9303e1) },
                { MinionSpawnPosition.SPAWN_RED_TOP, Tuple.Create(RED_TOP_WAYPOINTS, 0xffd23c3e) }
            };
            var cannonMinionCap = 2;

            foreach (var timestamp in cannonMinionTimestamps)
            {
                if (GameTime >= timestamp.Item1)
                {
                    cannonMinionCap = timestamp.Item2;
                }
            }

            foreach (var pos in positions)
            {
                var waypoints = spawnToWaypoints[pos].Item1;
                var inhibitorId = spawnToWaypoints[pos].Item2;
                var inhibitor = GetInhibitorById(inhibitorId);
                var isInhibitorDead = inhibitor.getState() == InhibitorState.Dead && !inhibitor.RespawnAnnounced;

                var list = REGULAR_MINION_WAVE;
                if (_cannonMinionCount >= cannonMinionCap)
                {
                    list = CANNON_MINION_WAVE;
                }

                if (isInhibitorDead)
                {
                    list = SUPER_MINION_WAVE;
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

        public override int GetMapId()
        {
            return 4;
        }

        public override int GetBluePillId()
        {
            return 2001;
        }

        public override float[] GetEndGameCameraPosition(TeamId team)
        {
            if (!_endGameCameraPosition.ContainsKey(team))
                return new float[] { 0, 0, 0 };

            return _endGameCameraPosition[team];
        }
    }
}
