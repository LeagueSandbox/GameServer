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
    class HowlingAbyss : Map
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
            { TeamId.TEAM_BLUE, new float[] { 1849.2987f, 1962.8167f, -192.4576f } },
            { TeamId.TEAM_PURPLE, new float[] { 10916.8809f, 10708.5723f, -198.1245f } }
        };

        public HowlingAbyss(Game game) : base(game, /*90*/5 * 1000, 30 * 1000, 90 * 1000, true, 1)
        {
            if (!RAFManager.getInstance().readAIMesh("LEVELS/Map12/AIPath.aimesh", out mesh))
            {
                Logger.LogCoreError("Failed to load Howling Abyss data.");
                return;
            }
            _collisionHandler.init(3); // Needs to be initialised after AIMesh

            AddObject(new Turret(game, game.GetNewNetID(), "Turret_OrderTurretShrine_A", 648.0914f, 764.2271f, TeamId.TEAM_BLUE, TurretType.FountainTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "Turret_T1_C_09_A", 2493.2317f, 2101.1807f, TeamId.TEAM_BLUE, TurretType.NexusTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "Turret_T1_C_010_A", 2036.6566f, 2552.6812f, TeamId.TEAM_BLUE, TurretType.NexusTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "Turret_T1_C_07_A", 3809.0610f, 3829.0515f, TeamId.TEAM_BLUE, TurretType.InhibitorTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "Turret_T1_C_08_A", 4943.4775f, 4929.8193f, TeamId.TEAM_BLUE, TurretType.InnerTurret));

            AddObject(new Turret(game, game.GetNewNetID(), "Turret_ChaosTurretShrine_A", 12168.7266f, 11913.2891f, TeamId.TEAM_PURPLE, TurretType.FountainTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "Turret_T2_L_04_A", 10325.2227f, 10608.1982f, TeamId.TEAM_PURPLE, TurretType.NexusTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "Turret_T2_L_03_A", 10785.1777f, 10117.5869f, TeamId.TEAM_PURPLE, TurretType.NexusTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "Turret_T2_L_02_A", 9017.6309f, 8871.3613f, TeamId.TEAM_PURPLE, TurretType.InhibitorTurret));
            AddObject(new Turret(game, game.GetNewNetID(), "Turret_T2_L_01_A", 7879.1025f, 7774.8018f, TeamId.TEAM_PURPLE, TurretType.InnerTurret));



            AddObject(new LevelProp(game, game.GetNewNetID(), 5026.5020f, 3496.3406f, -85.7209f, 134.0f, -122.2222f, 0.0f, 177.7777f, 111.1112f, "LevelProp_HA_AP_BridgeLaneStatue1", "HA_AP_BridgeLaneStatue"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 6466.1724f, 4860.2983f, -66.8992f, 134.0f, -144.4445f, 0.0f, 155.5557f, 144.4445f, "LevelProp_HA_AP_BridgeLaneStatue2", "HA_AP_BridgeLaneStatue"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 7899.2993f, 6244.1289f, -71.1058f, 136.0f, -122.2222f, 0.0f, 155.5557f, 133.3334f, "LevelProp_HA_AP_BridgeLaneStatue3", "HA_AP_BridgeLaneStatue"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 9322.6357f, 7618.5068f, -64.6960f, 134.0f, -133.3333f, 0.0f, 144.4445f, 122.2222f, "LevelProp_HA_AP_BridgeLaneStatue4", "HA_AP_BridgeLaneStatue"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 7807.8403f, 9235.3594f, -66.9590f, 316.0f, 111.1112f, 0.0f, 144.4445f, -111.1111f, "LevelProp_HA_AP_BridgeLaneStatue5", "HA_AP_BridgeLaneStatue"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 6344.9341f, 7901.9746f, -67.3494f, 316.0f, 144.4445f, 0.0f, 155.5557f, -155.5555f, "LevelProp_HA_AP_BridgeLaneStatue6", "HA_AP_BridgeLaneStatue"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 4911.4043f, 6519.0439f, -69.2768f, 318.0f, 144.4445f, 0.0f, 155.5557f, -166.6667f, "LevelProp_HA_AP_BridgeLaneStatue7", "HA_AP_BridgeLaneStatue"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 3488.6599f, 5143.9497f, -68.3749f, 316.0f, 144.4445f, 0.0f, 155.5557f, -144.4445f, "LevelProp_HA_AP_BridgeLaneStatue8", "HA_AP_BridgeLaneStatue"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 2883.209f, 5173.606f, 86.12982f, 144.0f, -88.8889f, 0.0f, 355.5555f, -100.0f, "LevelProp_HA_AP_Chains_Long", "HA_AP_Chains_Long"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 9939.937f, 7628.735f, 69.55461f, 320.0f, 111.1112f, 0.0f, 300.0f, -111.1111f, "LevelProp_HA_AP_Chains_Long1", "HA_AP_Chains_Long"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 4984.0581f, 3123.9731f, -269.4257f, 314.0f, 155.5557f, 0.0f, 366.6666f, -322.2222f, "LevelProp_HA_AP_Chains_Long2", "HA_AP_Chains_Long", 0x01)); // Needs Skin ID change, goes into reference pose after 20 seconds
            AddObject(new LevelProp(game, game.GetNewNetID(), 7757.7983f, 9702.5352f, -269.4257f, 134.0f, -33.3334f, 0.0f, 355.5555f, 166.6666f, "LevelProp_HA_AP_Chains_Long3", "HA_AP_Chains_Long", 0x01)); // Needs Skin ID change, goes into reference pose after 20 seconds
            AddObject(new LevelProp(game, game.GetNewNetID(), 3953.4500f, 6169.2261f, -134.2545f, 314.0f, -22.2222f, 0.0f, 222.2222f, -66.6667f, "LevelProp_HA_AP_Chains1", "HA_AP_Chains"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 5337.1914f, 7505.3691f, -134.2545f, 316.0f, 11.1111f, 0.0f, 211.1111f, 0.0f, "LevelProp_HA_AP_Chains2", "HA_AP_Chains"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 6804.2568f, 8921.9707f, -134.2545f, 316.0f, -33.3334f, 0.0f, 200.0f, -33.3334f, "LevelProp_HA_AP_Chains3", "HA_AP_Chains"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 7514.7021f, 5227.9307f, -134.2545f, 320.0f, -22.2222f, 0.0f, 211.1111f, 22.2223f, "LevelProp_HA_AP_Chains4", "HA_AP_Chains"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 8992.9395f, 6655.3198f, -134.2545f, 316.0f, -33.3334f, 0.0f, 222.2222f, -77.7778f, "LevelProp_HA_AP_Chains5", "HA_AP_Chains"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 6152.6162f, 3912.6973f, -134.2545f, 318.0f, -77.7778f, 0.0f, 222.2222f, -44.4445f, "LevelProp_HA_AP_Chains6", "HA_AP_Chains"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 8537.6924f, 7273.2739f, -399.9565f, 314.0f, -100.0f, 0.0f, 311.1111f, 433.3333f, "LevelProp_HA_AP_Cutaway", "HA_AP_Cutaway"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 11129.8574f, 12007.2168f, -208.8816f, 136.0f, 88.8889f, 0.0f, 44.4445f, 30.0f, "LevelProp_HA_AP_Hermit", "HA_AP_Hermit"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 11129.8574f, 12007.2168f, -208.8816f, 160.0f, 66.6667f, 0.0f, 0.0f, 122.2222f, "LevelProp_HA_AP_Hermit_Robot1", "HA_AP_Hermit_Robot"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 1637.6909f, 6079.6758f, -2986.0715f, 316.0f, 0.0f, 0.0f, -1000.0f, 0.0f, "LevelProp_HA_AP_HeroTower", "HA_AP_HeroTower"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 2493.2314f, 2101.1809f, -438.0855f, 230.0f, 0.0f, 0.0f, -33.3334f, 0.0f, "LevelProp_HA_AP_OrderCloth", "HA_AP_OrderCloth"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 2036.6564f, 2552.6814f, -434.7120f, 226.0f, 0.0f, 0.0f, -22.2222f, 0.0f, "LevelProp_HA_AP_OrderCloth1", "HA_AP_OrderCloth"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 3809.0608f, 3829.0518f, -410.9214f, 234.0f, 0.0f, 0.0f, -22.2222f, 0.0f, "LevelProp_HA_AP_OrderCloth2", "HA_AP_OrderCloth"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 4943.4775f, 4929.8188f, -415.1981f, 232.0f, 0.0f, 0.0f, -33.3334f, 0.0f, "LevelProp_HA_AP_OrderCloth3", "HA_AP_OrderCloth"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 110.9160f, 17282.7129f, -8541.3047f, 334.0f, -611.1111f, 0.0f, 322.2223f, 88.8889f, "LevelProp_HA_AP_PeriphBridge", "HA_AP_PeriphBridge"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 456.1896f, 593.4847f, 762.9286f, 136.0f, -44.4445f, 0.0f, -11.1111f, -77.7778f, "LevelProp_HA_AP_Poro", "HA_AP_Poro"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 2474.4504f, 4335.2720f, -57.8053f, 208.0f, -333.3333f, 0.0f, -55.5556f, 0.0f, "LevelProp_HA_AP_Poro1", "HA_AP_Poro"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 5733.5132f, 7807.1074f, -732.7032f, 34.0f, -88.8889f, 0.0f, 0.0f, 0.0f, "LevelProp_HA_AP_Poro2", "HA_AP_Poro"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 10992.3691f, 12432.2109f, -732.7032f, 166.0f, 44.4445f, 0.0f, 0.0f, 0.0f, "LevelProp_HA_AP_Poro3", "HA_AP_Poro"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 10388.6416f, 8398.5537f, -319.4668f, 222.0f, 266.6666f, 0.0f, -55.5556f, -11.1111f, "LevelProp_HA_AP_Poro4", "HA_AP_Poro"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 12845.7910f, 9704.9277f, -839.1965f, 182.0f, -288.8889f, 0.0f, -22.2222f, 244.4445f, "LevelProp_HA_AP_Poro5", "HA_AP_Poro"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 6776.0361f, 5412.7241f, 59.4416f, 130.0f, -22.2222f, 0.0f, -11.1111f, 0.0f, "LevelProp_HA_AP_Poro6", "HA_AP_Poro"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 11022.8477f, 12122.8457f, -106.7434f, 292.0f, -211.1111f, 0.0f, -111.1111f, -144.4445f, "LevelProp_HA_AP_ShpNorth", "HA_AP_ShpNorth"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 454.5473f, 1901.9037f, -208.8816f, 316.0f, 66.6667f, 0.0f, 22.2223f, 11.1111f, "LevelProp_HA_AP_ShpSouth", "HA_AP_ShpSouth"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 438.0326f, 1896.9458f, -208.8816f, 130.0f, 77.7777f, 0.0f, 111.1112f, 22.2223f, "LevelProp_HA_AP_Viking", "HA_AP_Viking"));
            AddObject(new LevelProp(game, game.GetNewNetID(), 7133.3125f, 5572.2632f, -868.6111f, 316.0f, -44.4445f, 0.0f, 111.1112f, 33.3333f, "LevelProp_HA_AP_BannerMidBridge", "HA_AP_BannerMidBridge"));



            //TODO
            var COLLISION_RADIUS = 0;
            var SIGHT_RANGE = 1700;

            AddObject(new Inhibitor(game, 0xffd23c3e, "Barracks_T1_C1", TeamId.TEAM_BLUE, COLLISION_RADIUS, 3128.5122f, 3171.5120f, SIGHT_RANGE));
            AddObject(new Inhibitor(game, 0xff6793d0, "Barracks_T2_C1", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 9699.4023f, 9515.8262f, SIGHT_RANGE));


            AddObject(new Nexus(game, 0xfff97db5, "HQ_T1", TeamId.TEAM_BLUE, COLLISION_RADIUS, 1849.2987f, 1962.8167f, SIGHT_RANGE));
            AddObject(new Nexus(game, 0xfff02c0f, "HQ_T2", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 10916.8809f, 10708.5723f, SIGHT_RANGE));

            // Start at xp to reach level 1
            _expToLevelUp = new List<int> { 0, 280, 660, 1140, 1720, 2400, 3180, 4060, 5040, 6120, 7300, 8580, 9960, 11440, 13020, 14700, 16480, 18360 };

            // Announcer events
            _announcerEvents.Add(new Announce(game, 30 * 1000, Announces.ThirySecondsToMinionsSpawn, true)); // HA uses ID 2
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
                    return new GameObjects.Target(11716, 11502);
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
            return 12;
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
