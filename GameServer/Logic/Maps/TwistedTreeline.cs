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
              new Vector2(12784.3525f, 9165.5205f),
              new Vector2(12857.7217f, 8509.4170f),
              new Vector2(12712.4707f, 7900.4546f),
           },
           new List<Vector2>
           { // blue bot
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
              new Vector2(12815.0693f, 5506.3154f),
              new Vector2(12838.4746f, 6113.4985f),
              new Vector2(12721.6514f, 6602.8179f)
           },
           new List<Vector2>
           { // red top
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
              new Vector2(2568.0986f, 9195.2891f),
              new Vector2(2498.1477f, 8539.1855f),
              new Vector2(2604.6230f, 7930.2227f),
           },
           new List<Vector2>
           { // red bot
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
              new Vector2(2699.3618f, 4963.8223f),
              new Vector2(2535.1941f, 5536.8584f),
              new Vector2(2483.4233f, 6144.0415f),
              new Vector2(2600.9341f, 6633.3608f)
           }
        };

        private Dictionary<TeamId, float[]> _endGameCameraPosition = new Dictionary<TeamId, float[]>
        {
            { TeamId.TEAM_BLUE, new float[] { 2981.0388f, 7283.0103f, 188 } },
            { TeamId.TEAM_PURPLE, new float[] { 12379.5439f, 7289.9409f, 110 } }
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
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_BLUE, new Vector2(2604.6230f, 7930.2227f));
                case MinionSpawnPosition.SPAWN_BLUE_BOT:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_BLUE, new Vector2(2600.9341f, 6633.3608f));
                case MinionSpawnPosition.SPAWN_RED_TOP:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_PURPLE, new Vector2(12712.4707f, 7900.4546f));
                case MinionSpawnPosition.SPAWN_RED_BOT:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_PURPLE, new Vector2(12721.6514f, 6602.8179f));
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
                MinionSpawnPosition.SPAWN_RED_TOP,
                MinionSpawnPosition.SPAWN_RED_BOT,
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
