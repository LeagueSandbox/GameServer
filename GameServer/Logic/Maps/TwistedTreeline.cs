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

            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T1_R_03_A", 10097.62f, 808.73f, TeamId.TEAM_BLUE, TurretType.OuterTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T1_R_02_A", 6512.53f, 1262.62f, TeamId.TEAM_BLUE, TurretType.InnerTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T1_R_01_A", 3747.26f, 1041.04f, TeamId.TEAM_BLUE, TurretType.InhibitorTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T2_R_03_A", 13459.0f, 4284.0f, TeamId.TEAM_PURPLE, TurretType.OuterTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T2_R_02_A", 12920.0f, 8005.0f, TeamId.TEAM_PURPLE, TurretType.InnerTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T2_R_01_A", 13205.0f, 10474.0f, TeamId.TEAM_PURPLE, TurretType.InhibitorTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T1_C_05_A", 5448.02f, 6169.10f, TeamId.TEAM_BLUE, TurretType.OuterTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T1_C_04_A", 4657.66f, 4591.91f, TeamId.TEAM_BLUE, TurretType.InnerTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T1_C_03_A", 3233.99f, 3447.24f, TeamId.TEAM_BLUE, TurretType.InhibitorTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T1_C_01_A", 1341.63f, 2029.98f, TeamId.TEAM_BLUE, TurretType.NexusTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T1_C_02_A", 1768.19f, 1589.47f, TeamId.TEAM_BLUE, TurretType.NexusTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T2_C_05_A", 8548.0f, 8289.0f, TeamId.TEAM_PURPLE, TurretType.OuterTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T2_C_04_A", 9361.0f, 9892.0f, TeamId.TEAM_PURPLE, TurretType.InnerTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T2_C_03_A", 10743.0f, 11010.0f, TeamId.TEAM_PURPLE, TurretType.InhibitorTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T2_C_01_A", 12662.0f, 12442.0f, TeamId.TEAM_PURPLE, TurretType.NexusTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T2_C_02_A", 12118.0f, 12876.0f, TeamId.TEAM_PURPLE, TurretType.NexusTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_OrderTurretShrine_A", -236.05f, -53.32f,TeamId.TEAM_BLUE, TurretType.FountainTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_ChaosTurretShrine_A", 14157.0f, 14456.0f, TeamId.TEAM_PURPLE, TurretType.FountainTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T1_L_03_A", 574.66f, 10220.47f, TeamId.TEAM_BLUE, TurretType.OuterTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T1_L_02_A", 1106.26f, 6485.25f, TeamId.TEAM_BLUE, TurretType.InnerTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T1_C_06_A", 802.81f, 4052.36f, TeamId.TEAM_BLUE, TurretType.InhibitorTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T2_L_03_A", 3911.0f, 13654.0f, TeamId.TEAM_PURPLE, TurretType.OuterTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T2_L_02_A", 7536.0f, 13190.0f, TeamId.TEAM_PURPLE, TurretType.InnerTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "@Turret_T2_L_01_A", 10261.0f, 13465.0f, TeamId.TEAM_PURPLE, TurretType.InhibitorTurret));

            AddObject(new LevelProp(game, game.GetNewNetID(), 14169.09f, 7916.989f, 178.1922f, 150f, 22.2223f, 0.0f, 33.3333f, -66.6667f, "LevelProp_TT_Shopkeeper1", "TT_Shopkeeper"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 1241.666f, 7916.235f, 184.2197f, 208f, -66.6667f, 0.0f, 22.2223f, -55.5556f, "LevelProp_TT_Shopkeeper", "TT_Shopkeeper"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 3624.281f, 3730.965f, -100.4387f, 0.0f, 88.8889f, 0.0f, -33.3334f, 66.6667f, "LevelProp_TT_Chains_Bot_Lane", "TT_Chains_Bot_Lane"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 3000.0f, 7289.682f, 19.51249f, 0.0f, 0.0f, 0.0f, 144.4445f, 0.0f, "LevelProp_TT_Nexus_Gears", "TT_Nexus_Gears"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 3624.281f, 3730.965f, -100.4387f, 0.0f, 88.8889f, 0.0f, -33.3334f, 66.6667f, "LevelProp_TT_Chains_Bot_Lane", "TT_Chains_Bot_Lane"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 3624.281f, 3730.965f, -100.4387f, 0.0f, 88.8889f, 0.0f, -33.3334f, 66.6667f, "LevelProp_TT_Chains_Bot_Lane", "TT_Chains_Bot_Lane"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 3624.281f, 3730.965f, -100.4387f, 0.0f, 88.8889f, 0.0f, -33.3334f, 66.6667f, "LevelProp_TT_Chains_Bot_Lane", "TT_Chains_Bot_Lane"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 3624.281f, 3730.965f, -100.4387f, 0.0f, 88.8889f, 0.0f, -33.3334f, 66.6667f, "LevelProp_TT_Chains_Bot_Lane", "TT_Chains_Bot_Lane"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 3624.281f, 3730.965f, -100.4387f, 0.0f, 88.8889f, 0.0f, -33.3334f, 66.6667f, "LevelProp_TT_Chains_Bot_Lane", "TT_Chains_Bot_Lane"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 3624.281f, 3730.965f, -100.4387f, 0.0f, 88.8889f, 0.0f, -33.3334f, 66.6667f, "LevelProp_TT_Chains_Bot_Lane", "TT_Chains_Bot_Lane"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 3624.281f, 3730.965f, -100.4387f, 0.0f, 88.8889f, 0.0f, -33.3334f, 66.6667f, "LevelProp_TT_Chains_Bot_Lane", "TT_Chains_Bot_Lane"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 3624.281f, 3730.965f, -100.4387f, 0.0f, 88.8889f, 0.0f, -33.3334f, 66.6667f, "LevelProp_TT_Chains_Bot_Lane", "TT_Chains_Bot_Lane"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 3624.281f, 3730.965f, -100.4387f, 0.0f, 88.8889f, 0.0f, -33.3334f, 66.6667f, "LevelProp_TT_Chains_Bot_Lane", "TT_Chains_Bot_Lane"));

            //TODO
            var COLLISION_RADIUS = 0;
            var SIGHT_RANGE = 1700;

            AddObject(new Inhibitor(game, 0xffd23c3e, "OrderInhibitor", TeamId.TEAM_BLUE, COLLISION_RADIUS, 835, 3400, SIGHT_RANGE)); //top
            AddObject(new Inhibitor(game, 0xff4a20f1, "OrderInhibitor", TeamId.TEAM_BLUE, COLLISION_RADIUS, 2785, 3000, SIGHT_RANGE)); //mid
            AddObject(new Inhibitor(game, 0xff9303e1, "OrderInhibitor", TeamId.TEAM_BLUE, COLLISION_RADIUS, 3044, 1070, SIGHT_RANGE)); //bot
            AddObject(new Inhibitor(game, 0xff6793d0, "ChaosInhibitor", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 10960, 13450, SIGHT_RANGE)); //top
            AddObject(new Inhibitor(game, 0xffff8f1f, "ChaosInhibitor", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 11240, 11490, SIGHT_RANGE)); //mid
            AddObject(new Inhibitor(game, 0xff26ac0f, "ChaosInhibitor", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 13200, 11200, SIGHT_RANGE)); //bot

            AddObject(new Nexus(game, 0xfff97db5, "OrderNexus", TeamId.TEAM_BLUE, COLLISION_RADIUS, 1170, 1470, SIGHT_RANGE));
            AddObject(new Nexus(game, 0xfff02c0f, "ChaosNexus", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 12800, 13100, SIGHT_RANGE));

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
                    return new GameObjects.Target(25.90f, 280);
                case 1:
                    return new GameObjects.Target(14119, 14063);
            }

            return new GameObjects.Target(25.90f, 280);
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

        public override int GetWidth()
        {
            return (int)mesh.getWidth();
        }

        public override int GetHeight()
        {
            return (int)mesh.getHeight();
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
