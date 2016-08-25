using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Core.Logic.RAF;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.Maps
{
    class TwistedTreeline : Map
    {
        private List<List<Vector2>> _laneWaypoints = new List<List<Vector2>>
        {
            new List<Vector2>
            { // blue top
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
           },
           new List<Vector2>
           { // blue bot
              new Vector2(1487.0f, 1302.0f),
              new Vector2(3789.0f, 1346.0f),
              new Vector2(6430.0f, 1005.0f),
              new Vector2(10995.0f, 1234.0f),
              new Vector2(12841.0f, 3051.0f),
              new Vector2(13148.0f, 4202.0f),
              new Vector2(13249.0f, 7884.0f),
              new Vector2(12886.0f, 10356.0f),
              new Vector2(12511.0f, 12776.0f)
           },
           new List<Vector2>
           { // blue mid
              new Vector2(1418.0f, 1686.0f),
              new Vector2(2997.0f, 2781.0f),
              new Vector2(4472.0f, 4727.0f),
              new Vector2(8375.0f, 8366.0f),
              new Vector2(10948.0f, 10821.0f),
              new Vector2(12511.0f, 12776.0f)
           },
           new List<Vector2>
           { // red top
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
           },
           new List<Vector2>
           { // red bot
              new Vector2(13062.0f, 12760.0f),
              new Vector2(12886.0f, 10356.0f),
              new Vector2(13249.0f, 7884.0f),
              new Vector2(13148.0f, 4202.0f),
              new Vector2(12841.0f, 3051.0f),
              new Vector2(10995.0f, 1234.0f),
              new Vector2(6430.0f, 1005.0f),
              new Vector2(3789.0f, 1346.0f),
              new Vector2(1418.0f, 1686.0f)
           },
           new List<Vector2>
           { // red mid
              new Vector2(12511.0f, 12776.0f),
              new Vector2(10948.0f, 10821.0f),
              new Vector2(8375.0f, 8366.0f),
              new Vector2(4472.0f, 4727.0f),
              new Vector2(2997.0f, 2781.0f),
              new Vector2(1418.0f, 1686.0f)
           }
        };

        private Dictionary<TeamId, float[]> _endGameCameraPosition = new Dictionary<TeamId, float[]>
        {
            { TeamId.TEAM_BLUE, new float[] { 1422, 1672, 188 } },
            { TeamId.TEAM_PURPLE, new float[] { 12500, 12800, 110 } }
        };

        public TwistedTreeline(Game game) : base(game, /*90*/5 * 1000, 30 * 1000, 90 * 1000, true, 1)
        {
            if (!RAFManager.getInstance().readAIMesh("LEVELS/Map10/AIPath.aimesh", out mesh))
            {
                Logger.LogCoreError("Failed to load TwistedTreeline data.");
                return;
            }
            _collisionHandler.init(3); // Needs to be initialised after AIMesh

            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_OrderTurretShrine_A", 295.03690f, 7271.2344f, TeamId.TEAM_BLUE, TurretType.FountainTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T1_C_01_A", 2407.5815f, 7288.8584f, TeamId.TEAM_BLUE, TurretType.NexusTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T1_C_06_A", 2135.5176f, 9264.0117f, TeamId.TEAM_BLUE, TurretType.InhibitorTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T1_C_07_A", 2130.2964f, 5241.2646f, TeamId.TEAM_BLUE, TurretType.InhibitorTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T1_L_02_A", 4426.5811f, 9726.0859f, TeamId.TEAM_BLUE, TurretType.InnerTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T1_R_02_A", 4645.6836f, 4718.1982f, TeamId.TEAM_BLUE, TurretType.InnerTurret));

            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_ChaosTurretShrine_A", 15020.6406f, 7301.6836f, TeamId.TEAM_PURPLE, TurretType.FountainTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T2_C_01_A", 13015.4688f, 7289.8652f, TeamId.TEAM_PURPLE, TurretType.NexusTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T2_L_01_A", 13291.2676f, 9260.7080f, TeamId.TEAM_PURPLE, TurretType.InhibitorTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T2_R_01_A", 13297.6621f, 5259.0078f, TeamId.TEAM_PURPLE, TurretType.InhibitorTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T2_R_02_A", 10775.8760f, 4715.4580f, TeamId.TEAM_PURPLE, TurretType.InnerTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T2_L_02_A", 10994.5430f, 9727.7715f, TeamId.TEAM_PURPLE, TurretType.InnerTurret));



            AddObject(new LevelProp(game, game.GetNewNetID(), 1360.9241f, 5072.1309f, 291.2142f, 134.0f, 11.1111f, 0.0f, 288.8889f, -22.2222f, "LevelProp_TT_Brazier1", "TT_Brazier"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 423.5712f, 6529.0327f, 385.9983f, 0.0f, -33.3334f, 0.0f, 277.7778f, -11.1111f, "LevelProp_TT_Brazier2", "TT_Brazier"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 399.4241f, 8021.057f, 692.2211f, 0.0f, -22.2222f, 0.0f, 300f, 0.0f, "LevelProp_TT_Brazier3", "TT_Brazier"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 1314.294f, 9495.576f, 582.8416f, 48.0f, -33.3334f, 0.0f, 277.7778f, 22.2223f, "LevelProp_TT_Brazier4", "TT_Brazier"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 14080.0f, 9530.3379f, 305.0638f, 120.0f, 11.1111f, 0.0f, 277.7778f, 0.0f, "LevelProp_TT_Brazier5", "TT_Brazier"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 14990.46f, 8053.91f, 675.8145f, 0.0f, -22.2222f, 0.0f, 266.6666f, -11.1111f, "LevelProp_TT_Brazier6", "TT_Brazier"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 15016.35f, 6532.84f, 664.7033f, 0.0f, -11.1111f, 0.0f, 255.5555f, -11.1111f, "LevelProp_TT_Brazier7", "TT_Brazier"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 14102.99f, 5098.367f, 580.504f, 36.0f, 0.0f, 0.0f, 244.4445f, 11.1111f, "LevelProp_TT_Brazier8", "TT_Brazier"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 3624.281f, 3730.965f, -100.4387f, 0.0f, 88.8889f, 0.0f, -33.3334f, 66.6667f, "LevelProp_TT_Chains_Bot_Lane", "TT_Chains_Bot_Lane"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 3778.364f, 7573.525f, -496.0713f, 0.0f, -233.3334f, 0.0f, -333.3333f, 277.7778f, "LevelProp_TT_Chains_Order_Base", "TT_Chains_Order_Base"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 11636.06f, 7618.667f, -551.6268f, 0.0f, 200f, 0.0f, -388.8889f, 33.3334f, "LevelProp_TT_Chains_Xaos_Base", "TT_Chains_Xaos_Base"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 759.1779f, 4740.938f, 507.9883f, 0.0f, -155.5555f, 0.0f, 44.4445f, 222.2222f, "LevelProp_TT_Chains_Order_Periph", "TT_Chains_Order_Periph"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 3000.0f, 7289.682f, 19.51249f, 0.0f, 0.0f, 0.0f, 144.4445f, 0.0f, "LevelProp_TT_Nexus_Gears", "TT_Nexus_Gears"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 12436.4775f, 7366.5859f, -124.9320f, 180.0f, -44.4445f, 0.0f, 122.2222f, -122.2222f, "LevelProp_TT_Nexus_Gears1", "TT_Nexus_Gears"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 14169.09f, 7916.989f, 178.1922f, 150f, 22.2223f, 0.0f, 33.3333f, -66.6667f, "LevelProp_TT_Shopkeeper1", "TT_Shopkeeper"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 1340.8141f, 7996.8691f, 126.2980f, 208f, -66.6667f, 0.0f, 22.2223f, -55.5556f, "LevelProp_TT_Shopkeeper", "TT_Shopkeeper"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 7706.3052f, 6720.3926f, -124.9320f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, "LevelProp_TT_Speedshrine_Gears", "TT_Speedshrine_Gears"));



            //TODO
            var COLLISION_RADIUS = 0;
            var SIGHT_RANGE = 1700;

            AddObject(new Inhibitor(game, 0xffd23c3e, "Barracks_T1_L1", TeamId.TEAM_BLUE, COLLISION_RADIUS, 2155.8677f, 8411.2500f, SIGHT_RANGE)); //top
            AddObject(new Inhibitor(game, 0xff9303e1, "Barracks_T1_R1", TeamId.TEAM_BLUE, COLLISION_RADIUS, 2147.5042f, 6117.3418f, SIGHT_RANGE)); //bot
            AddObject(new Inhibitor(game, 0xff6793d0, "Barracks_T2_L1", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 13284.7002f, 8408.0605f, SIGHT_RANGE)); //top
            AddObject(new Inhibitor(game, 0xff26ac0f, "Barracks_T2_R1", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 13295.3809f, 6124.8110f, SIGHT_RANGE)); //bot

            AddObject(new Nexus(game, 0xfff97db5, "HQ_T1", TeamId.TEAM_BLUE, COLLISION_RADIUS, 2981.0388f, 7283.0103f, SIGHT_RANGE));
            AddObject(new Nexus(game, 0xfff02c0f, "HQ_T2", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 12379.5439f, 7289.9409f, SIGHT_RANGE));

            // Start at xp to reach level 1
            _expToLevelUp = new List<int> { 0, 280, 660, 1140, 1720, 2400, 3180, 4060, 5040, 6120, 7300, 8580, 9960, 11440, 13020, 14700, 16480, 18360 };

            // Announcer events
            _announcerEvents.Add(new Announce(game, 30 * 1000, Announces.WelcomeToSR, true)); // Welcome to SR
            if (_firstSpawnTime - 30 * 1000 >= 0.0f)
                _announcerEvents.Add(new Announce(game, _firstSpawnTime - 30 * 1000, Announces.ThirySecondsToMinionsSpawn, true)); // 30 seconds until minions spawn
            _announcerEvents.Add(new Announce(game, _firstSpawnTime, Announces.MinionsHaveSpawned, false)); // Minions have spawned (90 * 1000)
            _announcerEvents.Add(new Announce(game, _firstSpawnTime, Announces.MinionsHaveSpawned2, false)); // Minions have spawned [2] (90 * 1000)
        }

        public override void Update(long diff)
        {
            base.Update(diff);

            if (_gameTime >= 120 * 1000)
            {
                SetKillReduction(false);
            }
        }
        public override float GetGoldPerSecond()
        {
            return 1.9f;
        }

        public override Target GetRespawnLocation(int team)
        {
            switch (team)
            {
                case 0:
                    return new GameObjects.Target(1051.19f, 7283.599f);
                case 1:
                    return new GameObjects.Target(14364, 7277);
            }

            return new GameObjects.Target(1051.19f, 7283.599f);
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
                if (c.getKillDeathCounter() < 5 && c.getKillDeathCounter() >= 0)
                {
                    if (c.getKillDeathCounter() == 0)
                        return gold;
                    for (int i = c.getKillDeathCounter(); i > 1; --i)
                        gold += gold * 0.165f;

                    return gold;
                }

                if (c.getKillDeathCounter() >= 5)
                    return 500.0f;

                if (c.getKillDeathCounter() < 0)
                {
                    float firstDeathGold = gold - gold * 0.085f;

                    if (c.getKillDeathCounter() == -1)
                        return firstDeathGold;

                    for (int i = c.getKillDeathCounter(); i < -1; ++i)
                        firstDeathGold -= firstDeathGold * 0.2f;

                    if (firstDeathGold < 50)
                        firstDeathGold = 50;

                    return firstDeathGold;
                }

                return 0.0f;
            }

            switch (m.getType())
            {
                case MinionSpawnType.MINION_TYPE_MELEE:
                    return 19.0f + ((0.5f) * (int)(_gameTime / (180 * 1000)));
                case MinionSpawnType.MINION_TYPE_CASTER:
                    return 14.0f + ((0.2f) * (int)(_gameTime / (90 * 1000)));
                case MinionSpawnType.MINION_TYPE_CANNON:
                    return 40.0f + ((1.0f) * (int)(_gameTime / (180 * 1000)));
            }

            return 0.0f;
        }
        public override float GetExperienceFor(Unit u)
        {
            var m = u as Minion;

            if (m == null)
                return 0.0f;

            switch (m.getType())
            {
                case MinionSpawnType.MINION_TYPE_MELEE:
                    return 58.88f;
                case MinionSpawnType.MINION_TYPE_CASTER:
                    return 29.44f;
                case MinionSpawnType.MINION_TYPE_CANNON:
                    return 92.0f;
            }

            return 0.0f;
        }

        public override Tuple<TeamId, Vector2> GetMinionSpawnPosition(MinionSpawnPosition spawnPosition)
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
        public override void SetMinionStats(Minion minion)
        {
            // Same for all minions
            minion.GetStats().MoveSpeed.BaseValue = 325.0f;
            minion.GetStats().AttackSpeedFlat = 0.625f;

            switch (minion.getType())
            {
                case MinionSpawnType.MINION_TYPE_MELEE:
                    minion.GetStats().CurrentHealth = 475.0f + 20.0f * (int)(_gameTime / (float)(180 * 1000));
                    minion.GetStats().HealthPoints.BaseValue = 475.0f + 20.0f * (int)(_gameTime / (float)(180 * 1000));
                    minion.GetStats().AttackDamage.BaseValue = 12.0f + 1.0f * (int)(_gameTime / (float)(180 * 1000));
                    minion.GetStats().Range.BaseValue = 180.0f;
                    minion.GetStats().AttackSpeedFlat = 1.250f;
                    minion.setAutoAttackDelay(11.8f / 30.0f);
                    minion.setMelee(true);
                    break;
                case MinionSpawnType.MINION_TYPE_CASTER:
                    minion.GetStats().CurrentHealth = 279.0f + 7.5f * (int)(_gameTime / (float)(90 * 1000));
                    minion.GetStats().HealthPoints.BaseValue = 279.0f + 7.5f * (int)(_gameTime / (float)(90 * 1000));
                    minion.GetStats().AttackDamage.BaseValue = 23.0f + 1.0f * (int)(_gameTime / (float)(90 * 1000));
                    minion.GetStats().Range.BaseValue = 600.0f;
                    minion.GetStats().AttackSpeedFlat = 0.670f;
                    minion.setAutoAttackDelay(14.1f / 30.0f);
                    minion.setAutoAttackProjectileSpeed(650.0f);
                    break;
                case MinionSpawnType.MINION_TYPE_CANNON:
                    minion.GetStats().CurrentHealth = 700.0f + 27.0f * (int)(_gameTime / (float)(180 * 1000));
                    minion.GetStats().HealthPoints.BaseValue = 700.0f + 27.0f * (int)(_gameTime / (float)(180 * 1000));
                    minion.GetStats().AttackDamage.BaseValue = 40.0f + 3.0f * (int)(_gameTime / (float)(180 * 1000));
                    minion.GetStats().Range.BaseValue = 450.0f;
                    minion.GetStats().AttackSpeedFlat = 1.0f;
                    minion.setAutoAttackDelay(9.0f / 30.0f);
                    minion.setAutoAttackProjectileSpeed(1200.0f);
                    break;
            }
        }

        public override bool Spawn()
        {
            var positions = new List<MinionSpawnPosition>
            {
                MinionSpawnPosition.SPAWN_BLUE_TOP,
                MinionSpawnPosition.SPAWN_BLUE_BOT,
                MinionSpawnPosition.SPAWN_BLUE_MID,
                MinionSpawnPosition.SPAWN_RED_TOP,
                MinionSpawnPosition.SPAWN_RED_BOT,
                MinionSpawnPosition.SPAWN_RED_MID,
            };

            if (_waveNumber < 3)
            {
                for (var i = 0; i < positions.Count; ++i)
                {
                    Minion m = new Minion(_game, _game.GetNewNetID(), MinionSpawnType.MINION_TYPE_MELEE, positions[i], _laneWaypoints[i]);
                    AddObject(m);
                }
                return false;
            }

            if (_waveNumber == 3)
            {
                for (var i = 0; i < positions.Count; ++i)
                {
                    Minion m = new Minion(_game, _game.GetNewNetID(), MinionSpawnType.MINION_TYPE_CANNON, positions[i], _laneWaypoints[i]);
                    AddObject(m);
                }
                return false;
            }

            if (_waveNumber < 7)
            {
                for (var i = 0; i < positions.Count; ++i)
                {
                    Minion m = new Minion(_game, _game.GetNewNetID(), MinionSpawnType.MINION_TYPE_CASTER, positions[i], _laneWaypoints[i]);
                    AddObject(m);
                }
                return false;
            }
            return true;
        }

        public override int GetMapId()
        {
            return 10;
        }

        public override Vector2 GetSize()
        {
            return new Vector2(GetWidth() / 2, GetHeight() / 2);
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
