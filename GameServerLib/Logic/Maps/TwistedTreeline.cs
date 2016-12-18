using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

namespace LeagueSandbox.GameServer.Logic.Maps
{
    class TwistedTreeline : Map
    {
        private Logger _logger = Program.ResolveDependency<Logger>();
        private int _cannonMinionCount;

        private static readonly List<Vector2> BLUE_TOP_WAYPOINTS = new List<Vector2>
        {
            new Vector2(2604.6230f, 7930.2227f),
            new Vector2(2498.1477f, 8539.1855f),
            new Vector2(2568.0986f, 9195.2891f),
            new Vector2(3070.7585f, 9779.2383f),
            new Vector2(3646.2886f, 9844.2227f),
            new Vector2(4192.0703f, 9804.6406f),
            new Vector2(4792.7705f, 9456.1240f),
            new Vector2(5371.9673f, 9104.9346f),
            new Vector2(6063.7998f, 8676.5762f),
            new Vector2(6569.7437f, 8453.1191f),
            new Vector2(7172.5576f, 8296.5186f),
            new Vector2(7679.0547f, 8227.8047f),
            new Vector2(8186.9468f, 8295.2314f),
            new Vector2(8795.4717f, 8445.9424f),
            new Vector2(9294.2402f, 8676.5762f),
            new Vector2(9986.0732f, 9104.9346f),
            new Vector2(10615.4189f, 9491.6660f),
            new Vector2(11185.9023f, 9803.4199f),
            new Vector2(11745.2314f, 9845.2314f),
            new Vector2(12290.2031f, 9776.3154f),
            new Vector2(13043.72f, 9313.694f),
            new Vector2(13069.5f, 8530.464f),
            new Vector2(12793.27f, 7427.271f)
        };

        private static readonly List<Vector2> BLUE_BOT_WAYPOINTS = new List<Vector2>
        {
            new Vector2(2600.9341f, 6633.3608f),
            new Vector2(2483.4233f, 6144.0415f),
            new Vector2(2535.1941f, 5536.8584f),
            new Vector2(2699.3618f, 4963.8223f),
            new Vector2(3614.5815f, 4588.3721f),
            new Vector2(4589.3037f, 4604.1772f),
            new Vector2(5630.8599f, 4814.9775f),
            new Vector2(7046.8311f, 5119.4941f),
            new Vector2(8309.6602f, 5119.4941f),
            new Vector2(9725.6309f, 4814.9775f),
            new Vector2(10767.1875f, 4604.1768f),
            new Vector2(11743.8223f, 4586.4595f),
            new Vector2(12523.4863f, 4984.4067f),
            new Vector2(13066.87f, 5400.705f),
            new Vector2(13041.69f, 6050.021f),
            new Vector2(12772.45f, 7260.062f)
        };

        private static readonly List<Vector2> RED_TOP_WAYPOINTS = new List<Vector2>
        {
            new Vector2(12712.4707f, 7900.4546f),
            new Vector2(12857.7217f, 8509.4170f),
            new Vector2(12784.3525f, 9165.5205f),
            new Vector2(12290.2031f, 9776.3154f),
            new Vector2(11745.2314f, 9845.2314f),
            new Vector2(11185.9023f, 9803.4199f),
            new Vector2(10615.4189f, 9491.6660f),
            new Vector2(9986.0732f, 9104.9346f),
            new Vector2(9294.2402f, 8676.5762f),
            new Vector2(8795.4717f, 8445.9424f),
            new Vector2(8186.9468f, 8295.2314f),
            new Vector2(7679.0547f, 8227.8047f),
            new Vector2(7172.5576f, 8296.5186f),
            new Vector2(6569.7437f, 8453.1191f),
            new Vector2(6063.7998f, 8676.5762f),
            new Vector2(5371.9673f, 9104.9346f),
            new Vector2(4792.7705f, 9456.1240f),
            new Vector2(4192.0703f, 9804.6406f),
            new Vector2(3646.2886f, 9844.2227f),
            new Vector2(3070.7585f, 9779.2383f),
            new Vector2(2299.986f, 9267.123f),
            new Vector2(2346.327f, 8533.396f),
            new Vector2(2611.366f, 7270.016f)
        };

        private static readonly List<Vector2> RED_BOT_WAYPOINTS = new List<Vector2>
        {
            new Vector2(12721.6514f, 6602.8179f),
            new Vector2(12838.4746f, 6113.4985f),
            new Vector2(12815.0693f, 5506.3154f),
            new Vector2(12523.4863f, 4984.4067f),
            new Vector2(11743.8223f, 4586.4595f),
            new Vector2(10767.1875f, 4604.1768f),
            new Vector2(9725.6309f, 4814.9775f),
            new Vector2(8309.6602f, 5119.4941f),
            new Vector2(7046.8311f, 5119.4941f),
            new Vector2(5630.8599f, 4814.9775f),
            new Vector2(4589.3037f, 4604.1772f),
            new Vector2(3614.5815f, 4588.3721f),
            new Vector2(2265.235f, 5341.108f),
            new Vector2(2314.38f, 5986.967f),
            new Vector2(2608.726f, 7246.868f)
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
            { TeamId.TEAM_BLUE, new[] { 2981.0388f, 7283.0103f, 188 } },
            { TeamId.TEAM_PURPLE, new[] { 12379.5439f, 7289.9409f, 110 } }
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

        public TwistedTreeline(Game game) : base(game, 90 * 1000, 30 * 1000, 90 * 1000, true, 1)
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
                _logger.LogCoreError("Failed to load Twisted Treeline data.");
                return;
            }

            AddObject(new LaneTurret("Turret_OrderTurretShrine_A", 295.03690f, 7271.2344f, TeamId.TEAM_BLUE, TurretType.FountainTurret));
            AddObject(new LaneTurret("Turret_T1_C_01_A", 2407.5815f, 7288.8584f, TeamId.TEAM_BLUE, TurretType.NexusTurret, GetTurretItems(TurretType.NexusTurret)));
            AddObject(new LaneTurret("Turret_T1_C_06_A", 2135.5176f, 9264.0117f, TeamId.TEAM_BLUE, TurretType.InhibitorTurret, GetTurretItems(TurretType.InhibitorTurret)));
            AddObject(new LaneTurret("Turret_T1_C_07_A", 2130.2964f, 5241.2646f, TeamId.TEAM_BLUE, TurretType.InhibitorTurret, GetTurretItems(TurretType.InhibitorTurret)));
            AddObject(new LaneTurret("Turret_T1_L_02_A", 4426.5811f, 9726.0859f, TeamId.TEAM_BLUE, TurretType.InnerTurret, GetTurretItems(TurretType.InnerTurret)));
            AddObject(new LaneTurret("Turret_T1_R_02_A", 4645.6836f, 4718.1982f, TeamId.TEAM_BLUE, TurretType.InnerTurret, GetTurretItems(TurretType.InnerTurret)));

            AddObject(new LaneTurret("Turret_ChaosTurretShrine_A", 15020.6406f, 7301.6836f, TeamId.TEAM_PURPLE, TurretType.FountainTurret));
            AddObject(new LaneTurret("Turret_T2_C_01_A", 13015.4688f, 7289.8652f, TeamId.TEAM_PURPLE, TurretType.NexusTurret, GetTurretItems(TurretType.NexusTurret)));
            AddObject(new LaneTurret("Turret_T2_L_01_A", 13291.2676f, 9260.7080f, TeamId.TEAM_PURPLE, TurretType.InhibitorTurret, GetTurretItems(TurretType.InhibitorTurret)));
            AddObject(new LaneTurret("Turret_T2_R_01_A", 13297.6621f, 5259.0078f, TeamId.TEAM_PURPLE, TurretType.InhibitorTurret, GetTurretItems(TurretType.InhibitorTurret)));
            AddObject(new LaneTurret("Turret_T2_R_02_A", 10775.8760f, 4715.4580f, TeamId.TEAM_PURPLE, TurretType.InnerTurret, GetTurretItems(TurretType.InnerTurret)));
            AddObject(new LaneTurret("Turret_T2_L_02_A", 10994.5430f, 9727.7715f, TeamId.TEAM_PURPLE, TurretType.InnerTurret, GetTurretItems(TurretType.InnerTurret)));

            AddObject(new LevelProp(1360.9241f, 5072.1309f, 291.2142f, 134.0f, 11.1111f, 0.0f, 288.8889f, -22.2222f, "LevelProp_TT_Brazier1", "TT_Brazier"));
            AddObject(new LevelProp(423.5712f, 6529.0327f, 385.9983f, 0.0f, -33.3334f, 0.0f, 277.7778f, -11.1111f, "LevelProp_TT_Brazier2", "TT_Brazier"));
            AddObject(new LevelProp(399.4241f, 8021.057f, 692.2211f, 0.0f, -22.2222f, 0.0f, 300f, 0.0f, "LevelProp_TT_Brazier3", "TT_Brazier"));
            AddObject(new LevelProp(1314.294f, 9495.576f, 582.8416f, 48.0f, -33.3334f, 0.0f, 277.7778f, 22.2223f, "LevelProp_TT_Brazier4", "TT_Brazier"));
            AddObject(new LevelProp(14080.0f, 9530.3379f, 305.0638f, 120.0f, 11.1111f, 0.0f, 277.7778f, 0.0f, "LevelProp_TT_Brazier5", "TT_Brazier"));
            AddObject(new LevelProp(14990.46f, 8053.91f, 675.8145f, 0.0f, -22.2222f, 0.0f, 266.6666f, -11.1111f, "LevelProp_TT_Brazier6", "TT_Brazier"));
            AddObject(new LevelProp(15016.35f, 6532.84f, 664.7033f, 0.0f, -11.1111f, 0.0f, 255.5555f, -11.1111f, "LevelProp_TT_Brazier7", "TT_Brazier"));
            AddObject(new LevelProp(14102.99f, 5098.367f, 580.504f, 36.0f, 0.0f, 0.0f, 244.4445f, 11.1111f, "LevelProp_TT_Brazier8", "TT_Brazier"));
            AddObject(new LevelProp(3624.281f, 3730.965f, -100.4387f, 0.0f, 88.8889f, 0.0f, -33.3334f, 66.6667f, "LevelProp_TT_Chains_Bot_Lane", "TT_Chains_Bot_Lane"));
            AddObject(new LevelProp(3778.364f, 7573.525f, -496.0713f, 0.0f, -233.3334f, 0.0f, -333.3333f, 277.7778f, "LevelProp_TT_Chains_Order_Base", "TT_Chains_Order_Base"));
            AddObject(new LevelProp(11636.06f, 7618.667f, -551.6268f, 0.0f, 200f, 0.0f, -388.8889f, 33.3334f, "LevelProp_TT_Chains_Xaos_Base", "TT_Chains_Xaos_Base"));
            AddObject(new LevelProp(759.1779f, 4740.938f, 507.9883f, 0.0f, -155.5555f, 0.0f, 44.4445f, 222.2222f, "LevelProp_TT_Chains_Order_Periph", "TT_Chains_Order_Periph"));
            AddObject(new LevelProp(3000.0f, 7289.682f, 19.51249f, 0.0f, 0.0f, 0.0f, 144.4445f, 0.0f, "LevelProp_TT_Nexus_Gears", "TT_Nexus_Gears"));
            AddObject(new LevelProp(12436.4775f, 7366.5859f, -124.9320f, 180.0f, -44.4445f, 0.0f, 122.2222f, -122.2222f, "LevelProp_TT_Nexus_Gears1", "TT_Nexus_Gears"));
            AddObject(new LevelProp(14169.09f, 7916.989f, 178.1922f, 150f, 22.2223f, 0.0f, 33.3333f, -66.6667f, "LevelProp_TT_Shopkeeper1", "TT_Shopkeeper"));
            AddObject(new LevelProp(1340.8141f, 7996.8691f, 126.2980f, 208f, -66.6667f, 0.0f, 22.2223f, -55.5556f, "LevelProp_TT_Shopkeeper", "TT_Shopkeeper"));
            AddObject(new LevelProp(7706.3052f, 6720.3926f, -124.9320f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, "LevelProp_TT_Speedshrine_Gears", "TT_Speedshrine_Gears"));

            //TODO
            var COLLISION_RADIUS = 0;
            var SIGHT_RANGE = 1700;

            AddObject(new Inhibitor("OrderInhibitor", TeamId.TEAM_BLUE, COLLISION_RADIUS, 2155.8677f, 8411.2500f, SIGHT_RANGE, 0xffd23c3e)); //top
            AddObject(new Inhibitor("OrderInhibitor", TeamId.TEAM_BLUE, COLLISION_RADIUS, 2147.5042f, 6117.3418f, SIGHT_RANGE, 0xff9303e1)); //bot
            AddObject(new Inhibitor("ChaosInhibitor", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 13284.7002f, 8408.0605f, SIGHT_RANGE, 0xff6793d0)); //top
            AddObject(new Inhibitor("ChaosInhibitor", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 13295.3809f, 6124.8110f, SIGHT_RANGE, 0xff26ac0f)); //bot

            AddObject(new Nexus("OrderNexus", TeamId.TEAM_BLUE, COLLISION_RADIUS, 2981.0388f, 7283.0103f, SIGHT_RANGE, 0xfff97db5));
            AddObject(new Nexus("ChaosNexus", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 12379.5439f, 7289.9409f, SIGHT_RANGE, 0xfff02c0f));

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
                { TeamId.TEAM_PURPLE, new Target(14364, 7277) }
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
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_BLUE, new Vector2(2604, 7930));
                case MinionSpawnPosition.SPAWN_BLUE_BOT:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_BLUE, new Vector2(2600, 6633));
                case MinionSpawnPosition.SPAWN_RED_TOP:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_PURPLE, new Vector2(12712, 7900));
                case MinionSpawnPosition.SPAWN_RED_BOT:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_PURPLE, new Vector2(12721, 6602));
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
            return 10;
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