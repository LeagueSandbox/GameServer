using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace LeagueSandbox.GameServer.Logic.Maps
{
    class HowlingAbyss : Map
    {
        private Logger _logger = Program.ResolveDependency<Logger>();
        private int _cannonMinionCount;
        private static readonly List<Vector2> BLUE_MID_WAYPOINTS = new List<Vector2>
        {
            new Vector2(1996.345f, 2324.655f),
            new Vector2(2302.238f, 2407.359f),
            new Vector2(2945.391f, 3270.245f),
            new Vector2(3703.784f, 4016.463f),
            new Vector2(4828.833f, 5093.357f),
            new Vector2(6376.11f, 6264.086f),
            new Vector2(8013.089f, 7666.26f),
            new Vector2(9157.47f, 8760.058f),
            new Vector2(9824.76f, 9349.601f),
            new Vector2(10570.63f, 10368.79f),
            new Vector2(10835.05f, 10376.95f)
        };

        private static readonly List<Vector2> RED_MID_WAYPOINTS = new List<Vector2>
        {
            new Vector2(10835.05f, 10376.95f),
            new Vector2(10570.63f, 10368.79f),
            new Vector2(9824.76f, 9349.601f),
            new Vector2(9157.47f, 8760.058f),
            new Vector2(8013.089f, 7666.26f),
            new Vector2(6376.11f, 6264.086f),
            new Vector2(4828.833f, 5093.357f),
            new Vector2(3703.784f, 4016.463f),
            new Vector2(2945.391f, 3270.245f),
            new Vector2(2302.238f, 2407.359f),
            new Vector2(1996.345f, 2324.655f)
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
            { TeamId.TEAM_BLUE, new[] { 1849.2987f, 1962.8167f, -192.4576f } },
            { TeamId.TEAM_PURPLE, new[] { 10916.8809f, 10708.5723f, -198.1245f } }
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

        public HowlingAbyss(Game game) : base(game, 90 * 1000, 30 * 1000, 90 * 1000, false, 12)
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
                _logger.LogCoreError("Failed to load Howling Abyss data.");
                return;
            }

            AddObject(new LaneTurret("Turret_OrderTurretShrine_A", 648.0914f, 764.2271f, TeamId.TEAM_BLUE, TurretType.FountainTurret));
            AddObject(new LaneTurret("Turret_T1_C_09_A", 2493.2317f, 2101.1807f, TeamId.TEAM_BLUE, TurretType.NexusTurret, GetTurretItems(TurretType.NexusTurret)));
            AddObject(new LaneTurret("Turret_T1_C_010_A", 2036.6566f, 2552.6812f, TeamId.TEAM_BLUE, TurretType.NexusTurret, GetTurretItems(TurretType.NexusTurret)));
            AddObject(new LaneTurret("Turret_T1_C_07_A", 3809.0610f, 3829.0515f, TeamId.TEAM_BLUE, TurretType.InhibitorTurret, GetTurretItems(TurretType.InhibitorTurret)));
            AddObject(new LaneTurret("Turret_T1_C_08_A", 4943.4775f, 4929.8193f, TeamId.TEAM_BLUE, TurretType.InnerTurret, GetTurretItems(TurretType.InnerTurret)));

            AddObject(new LaneTurret("Turret_ChaosTurretShrine_A", 12168.7266f, 11913.2891f, TeamId.TEAM_PURPLE, TurretType.FountainTurret));
            AddObject(new LaneTurret("Turret_T2_L_04_A", 10325.2227f, 10608.1982f, TeamId.TEAM_PURPLE, TurretType.NexusTurret, GetTurretItems(TurretType.NexusTurret)));
            AddObject(new LaneTurret("Turret_T2_L_03_A", 10785.1777f, 10117.5869f, TeamId.TEAM_PURPLE, TurretType.NexusTurret, GetTurretItems(TurretType.NexusTurret)));
            AddObject(new LaneTurret("Turret_T2_L_02_A", 9017.6309f, 8871.3613f, TeamId.TEAM_PURPLE, TurretType.InhibitorTurret, GetTurretItems(TurretType.InhibitorTurret)));
            AddObject(new LaneTurret("Turret_T2_L_01_A", 7879.1025f, 7774.8018f, TeamId.TEAM_PURPLE, TurretType.InnerTurret, GetTurretItems(TurretType.InnerTurret)));

            AddObject(new LevelProp(5026.5020f, 3496.3406f, -85.7209f, 134.0f, -122.2222f, 0.0f, 177.7777f, 111.1112f, "LevelProp_HA_AP_BridgeLaneStatue1", "HA_AP_BridgeLaneStatue"));
            AddObject(new LevelProp(6466.1724f, 4860.2983f, -66.8992f, 134.0f, -144.4445f, 0.0f, 155.5557f, 144.4445f, "LevelProp_HA_AP_BridgeLaneStatue2", "HA_AP_BridgeLaneStatue"));
            AddObject(new LevelProp(7899.2993f, 6244.1289f, -71.1058f, 136.0f, -122.2222f, 0.0f, 155.5557f, 133.3334f, "LevelProp_HA_AP_BridgeLaneStatue3", "HA_AP_BridgeLaneStatue"));
            AddObject(new LevelProp(9322.6357f, 7618.5068f, -64.6960f, 134.0f, -133.3333f, 0.0f, 144.4445f, 122.2222f, "LevelProp_HA_AP_BridgeLaneStatue4", "HA_AP_BridgeLaneStatue"));
            AddObject(new LevelProp(7807.8403f, 9235.3594f, -66.9590f, 316.0f, 111.1112f, 0.0f, 144.4445f, -111.1111f, "LevelProp_HA_AP_BridgeLaneStatue5", "HA_AP_BridgeLaneStatue"));
            AddObject(new LevelProp(6344.9341f, 7901.9746f, -67.3494f, 316.0f, 144.4445f, 0.0f, 155.5557f, -155.5555f, "LevelProp_HA_AP_BridgeLaneStatue6", "HA_AP_BridgeLaneStatue"));
            AddObject(new LevelProp(4911.4043f, 6519.0439f, -69.2768f, 318.0f, 144.4445f, 0.0f, 155.5557f, -166.6667f, "LevelProp_HA_AP_BridgeLaneStatue7", "HA_AP_BridgeLaneStatue"));
            AddObject(new LevelProp(3488.6599f, 5143.9497f, -68.3749f, 316.0f, 144.4445f, 0.0f, 155.5557f, -144.4445f, "LevelProp_HA_AP_BridgeLaneStatue8", "HA_AP_BridgeLaneStatue"));
            AddObject(new LevelProp(2883.209f, 5173.606f, 86.12982f, 144.0f, -88.8889f, 0.0f, 355.5555f, -100.0f, "LevelProp_HA_AP_Chains_Long", "HA_AP_Chains_Long"));
            AddObject(new LevelProp(9939.937f, 7628.735f, 69.55461f, 320.0f, 111.1112f, 0.0f, 300.0f, -111.1111f, "LevelProp_HA_AP_Chains_Long1", "HA_AP_Chains_Long"));
            AddObject(new LevelProp(4984.0581f, 3123.9731f, -269.4257f, 314.0f, 155.5557f, 0.0f, 366.6666f, -322.2222f, "LevelProp_HA_AP_Chains_Long2", "HA_AP_Chains_Long", 1)); // Needs Skin ID change, goes into reference pose after 20 seconds
            AddObject(new LevelProp(7757.7983f, 9702.5352f, -269.4257f, 134.0f, -33.3334f, 0.0f, 355.5555f, 166.6666f, "LevelProp_HA_AP_Chains_Long3", "HA_AP_Chains_Long", 1)); // Needs Skin ID change, goes into reference pose after 20 seconds
            AddObject(new LevelProp(3953.4500f, 6169.2261f, -134.2545f, 314.0f, -22.2222f, 0.0f, 222.2222f, -66.6667f, "LevelProp_HA_AP_Chains1", "HA_AP_Chains"));
            AddObject(new LevelProp(5337.1914f, 7505.3691f, -134.2545f, 316.0f, 11.1111f, 0.0f, 211.1111f, 0.0f, "LevelProp_HA_AP_Chains2", "HA_AP_Chains"));
            AddObject(new LevelProp(6804.2568f, 8921.9707f, -134.2545f, 316.0f, -33.3334f, 0.0f, 200.0f, -33.3334f, "LevelProp_HA_AP_Chains3", "HA_AP_Chains"));
            AddObject(new LevelProp(7514.7021f, 5227.9307f, -134.2545f, 320.0f, -22.2222f, 0.0f, 211.1111f, 22.2223f, "LevelProp_HA_AP_Chains4", "HA_AP_Chains"));
            AddObject(new LevelProp(8992.9395f, 6655.3198f, -134.2545f, 316.0f, -33.3334f, 0.0f, 222.2222f, -77.7778f, "LevelProp_HA_AP_Chains5", "HA_AP_Chains"));
            AddObject(new LevelProp(6152.6162f, 3912.6973f, -134.2545f, 318.0f, -77.7778f, 0.0f, 222.2222f, -44.4445f, "LevelProp_HA_AP_Chains6", "HA_AP_Chains"));
            AddObject(new LevelProp(8537.6924f, 7273.2739f, -399.9565f, 314.0f, -100.0f, 0.0f, 311.1111f, 433.3333f, "LevelProp_HA_AP_Cutaway", "HA_AP_Cutaway"));
            AddObject(new LevelProp(11129.8574f, 12007.2168f, -208.8816f, 136.0f, 88.8889f, 0.0f, 44.4445f, 30.0f, "LevelProp_HA_AP_Hermit", "HA_AP_Hermit"));
            AddObject(new LevelProp(11129.8574f, 12007.2168f, -208.8816f, 160.0f, 66.6667f, 0.0f, 0.0f, 122.2222f, "LevelProp_HA_AP_Hermit_Robot1", "HA_AP_Hermit_Robot"));
            AddObject(new LevelProp(1637.6909f, 6079.6758f, -2986.0715f, 316.0f, 0.0f, 0.0f, -1000.0f, 0.0f, "LevelProp_HA_AP_HeroTower", "HA_AP_HeroTower"));
            AddObject(new LevelProp(2493.2314f, 2101.1809f, -438.0855f, 230.0f, 0.0f, 0.0f, -33.3334f, 0.0f, "LevelProp_HA_AP_OrderCloth", "HA_AP_OrderCloth"));
            AddObject(new LevelProp(2036.6564f, 2552.6814f, -434.7120f, 226.0f, 0.0f, 0.0f, -22.2222f, 0.0f, "LevelProp_HA_AP_OrderCloth1", "HA_AP_OrderCloth"));
            AddObject(new LevelProp(3809.0608f, 3829.0518f, -410.9214f, 234.0f, 0.0f, 0.0f, -22.2222f, 0.0f, "LevelProp_HA_AP_OrderCloth2", "HA_AP_OrderCloth"));
            AddObject(new LevelProp(4943.4775f, 4929.8188f, -415.1981f, 232.0f, 0.0f, 0.0f, -33.3334f, 0.0f, "LevelProp_HA_AP_OrderCloth3", "HA_AP_OrderCloth"));
            AddObject(new LevelProp(110.9160f, 17282.7129f, -8541.3047f, 334.0f, -611.1111f, 0.0f, 322.2223f, 88.8889f, "LevelProp_HA_AP_PeriphBridge", "HA_AP_PeriphBridge"));
            AddObject(new LevelProp(456.1896f, 593.4847f, 762.9286f, 136.0f, -44.4445f, 0.0f, -11.1111f, -77.7778f, "LevelProp_HA_AP_Poro", "HA_AP_Poro"));
            AddObject(new LevelProp(2474.4504f, 4335.2720f, -57.8053f, 208.0f, -333.3333f, 0.0f, -55.5556f, 0.0f, "LevelProp_HA_AP_Poro1", "HA_AP_Poro"));
            AddObject(new LevelProp(5733.5132f, 7807.1074f, -732.7032f, 34.0f, -88.8889f, 0.0f, 0.0f, 0.0f, "LevelProp_HA_AP_Poro2", "HA_AP_Poro"));
            AddObject(new LevelProp(10992.3691f, 12432.2109f, -732.7032f, 166.0f, 44.4445f, 0.0f, 0.0f, 0.0f, "LevelProp_HA_AP_Poro3", "HA_AP_Poro"));
            AddObject(new LevelProp(10388.6416f, 8398.5537f, -319.4668f, 222.0f, 266.6666f, 0.0f, -55.5556f, -11.1111f, "LevelProp_HA_AP_Poro4", "HA_AP_Poro"));
            AddObject(new LevelProp(12845.7910f, 9704.9277f, -839.1965f, 182.0f, -288.8889f, 0.0f, -22.2222f, 244.4445f, "LevelProp_HA_AP_Poro5", "HA_AP_Poro"));
            AddObject(new LevelProp(6776.0361f, 5412.7241f, 59.4416f, 130.0f, -22.2222f, 0.0f, -11.1111f, 0.0f, "LevelProp_HA_AP_Poro6", "HA_AP_Poro"));
            AddObject(new LevelProp(11022.8477f, 12122.8457f, -106.7434f, 292.0f, -211.1111f, 0.0f, -111.1111f, -144.4445f, "LevelProp_HA_AP_ShpNorth", "HA_AP_ShpNorth"));
            AddObject(new LevelProp(454.5473f, 1901.9037f, -208.8816f, 316.0f, 66.6667f, 0.0f, 22.2223f, 11.1111f, "LevelProp_HA_AP_ShpSouth", "HA_AP_ShpSouth"));
            AddObject(new LevelProp(438.0326f, 1896.9458f, -208.8816f, 130.0f, 77.7777f, 0.0f, 111.1112f, 22.2223f, "LevelProp_HA_AP_Viking", "HA_AP_Viking"));
            AddObject(new LevelProp(7133.3125f, 5572.2632f, -868.6111f, 316.0f, -44.4445f, 0.0f, 111.1112f, 33.3333f, "LevelProp_HA_AP_BannerMidBridge", "HA_AP_BannerMidBridge"));

            //TODO
            var COLLISION_RADIUS = 0;
            var SIGHT_RANGE = 1700;

            AddObject(new Inhibitor("OrderInhibitor", TeamId.TEAM_BLUE, COLLISION_RADIUS, 3128.5122f, 3171.5120f, SIGHT_RANGE, 0xff4a20f1));
            AddObject(new Inhibitor("ChaosInhibitor", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 9699.4023f, 9515.8262f, SIGHT_RANGE, 0xffff8f1f));


            AddObject(new Nexus("OrderNexus", TeamId.TEAM_BLUE, COLLISION_RADIUS, 1849.2987f, 1962.8167f, SIGHT_RANGE, 0xfff97db5));
            AddObject(new Nexus("ChaosNexus", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 10916.8809f, 10708.5723f, SIGHT_RANGE, 0xfff02c0f));

            // Start at xp to reach level 1
            ExpToLevelUp = new List<int> { 0, 280, 660, 1140, 1720, 2400, 3180, 4060, 5040, 6120, 7300, 8580, 9960, 11440, 13020, 14700, 16480, 18360 };

            // Announcer events
            _announcerEvents.Add(new Announce(game, 30 * 1000, Announces.ThirySecondsToMinionsSpawn, true)); // HA uses ID 2
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

        public override void Update(long diff)
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
                { TeamId.TEAM_BLUE, new Target(1051.19f, 7283.599f) },
                { TeamId.TEAM_PURPLE, new Target(11716, 11502) }
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

                    for (var i = c.KillDeathCounter; i > 1; --i)
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
                { MinionSpawnType.MINION_TYPE_MELEE, 19.0f + 0.5f * (int)(GameTime/(180 * 1000)) },
                { MinionSpawnType.MINION_TYPE_CASTER, 14.0f + 0.2f * (int)(GameTime/(90 * 1000)) },
                { MinionSpawnType.MINION_TYPE_CANNON, 40.0f + 1.0f * (int)(GameTime/(180 * 1000)) },
                { MinionSpawnType.MINION_TYPE_SUPER, 40.0f + 1.0f * (int)(GameTime/(180 * 1000)) }
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
                case MinionSpawnPosition.SPAWN_BLUE_MID:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_BLUE, new Vector2(1996, 2324));
                case MinionSpawnPosition.SPAWN_RED_MID:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_PURPLE, new Vector2(10835, 10376));
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
                MinionSpawnPosition.SPAWN_BLUE_MID,
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
                {MinionSpawnPosition.SPAWN_BLUE_MID, Tuple.Create(BLUE_MID_WAYPOINTS, 0xffff8f1f)},
                {MinionSpawnPosition.SPAWN_RED_MID, Tuple.Create(RED_MID_WAYPOINTS, 0xff4a20f1)}
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
            return 12;
        }

        public override int GetBluePillId()
        {
            return 2001;
        }

        public override float[] GetEndGameCameraPosition(TeamId team)
        {
            if (!_endGameCameraPosition.ContainsKey(team))
                return new[] { 0f, 0f, 0f };

            return _endGameCameraPosition[team];
        }
    }
}