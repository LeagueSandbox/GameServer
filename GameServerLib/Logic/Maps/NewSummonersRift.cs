using LeagueSandbox.GameServer.Core.Logic;
using LeagueSandbox.GameServer.Logic.Enet;
using LeagueSandbox.GameServer.Logic.GameObjects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using LeagueSandbox.GameServer.Logic.RAF;

namespace LeagueSandbox.GameServer.Logic.Maps
{
    class NewSummonersRift : Map
    {
        private static readonly List<Vector2> BLUE_TOP_WAYPOINTS = new List<Vector2>
        {
            new Vector2(1109.416f, 2091.567f),
            new Vector2(1508.93f, 3597.096f),
            new Vector2(1382.255f, 4910.181f),
            new Vector2(1170.487f, 6715.997f),
            new Vector2(1207.795f, 7787.871f),
            new Vector2(1229.664f, 9072.834f),
            new Vector2(1285.232f, 10423.6f),
            new Vector2(1508.611f, 11601.36f),
            new Vector2(1968.444f, 12459.81f),
            new Vector2(2720.787f, 13100.3f),
            new Vector2(3486.236f, 13473.17f),
            new Vector2(4359.079f, 13597.91f),
            new Vector2(5504.224f, 13608.08f),
            new Vector2(6668.632f, 13647.62f),
            new Vector2(8113.482f, 13665.75f),
            new Vector2(9889.146f, 13447.13f),
            new Vector2(11348.6f, 13344.28f),
            new Vector2(13473.82f, 13444.93f)
        };
        private static readonly List<Vector2> BLUE_BOT_WAYPOINTS = new List<Vector2>
        {
            new Vector2(2034.091f, 1171.083f),
            new Vector2(3471.763f, 1589.286f),
            new Vector2(4844.017f, 1440.128f),
            new Vector2(6925.2f, 1163.12f),
            new Vector2(7975.36f, 1230.857f),
            new Vector2(8870.221f, 1212.785f),
            new Vector2(10506.54f, 1301.828f),
            new Vector2(11432.13f, 1472.96f),
            new Vector2(12294.41f, 1913.186f),
            new Vector2(13001.41f, 2628.15f),
            new Vector2(13529.33f, 3694.875f),
            new Vector2(13751.53f, 4790.221f),
            new Vector2(13741.61f, 5811.374f),
            new Vector2(13710.52f, 7177.168f),
            new Vector2(13696.65f, 8608.403f),
            new Vector2(13364.14f, 10040.18f),
            new Vector2(13253.06f, 11334.05f),
            new Vector2(13473.82f, 13444.93f)
        };
        private static readonly List<Vector2> BLUE_MID_WAYPOINTS = new List<Vector2>
        {
            new Vector2(2008.126f, 2079.599f),
            new Vector2(2952.614f, 3474.726f),
            new Vector2(3856.273f, 4170.232f),
            new Vector2(4874.661f, 5141.459f),
            new Vector2(5375.718f, 5589.086f),
            new Vector2(5823.8f, 5928.047f),
            new Vector2(6270.211f, 6366.785f),
            new Vector2(6764.715f, 6864.42f),
            new Vector2(7408.699f, 7405.615f),
            new Vector2(8079.619f, 8022.976f),
            new Vector2(8618.293f, 8647.033f),
            new Vector2(9149.958f, 9105.284f),
            new Vector2(9625.553f, 9543.4f),
            new Vector2(10193.23f, 10035.72f),
            new Vector2(10822.95f, 10646.96f),
            new Vector2(11808.24f, 11479),
            new Vector2(13473.82f, 13444.93f)
        };
        private static readonly List<Vector2> RED_TOP_WAYPOINTS = new List<Vector2>
        {
            new Vector2(12800.61f, 13745.36f),
            new Vector2(11348.6f, 13344.28f),
            new Vector2(9889.146f, 13447.13f),
            new Vector2(8113.482f, 13665.75f),
            new Vector2(6668.632f, 13647.62f),
            new Vector2(5504.224f, 13608.08f),
            new Vector2(4359.079f, 13597.91f),
            new Vector2(3486.236f, 13473.17f),
            new Vector2(2720.787f, 13100.3f),
            new Vector2(1968.444f, 12459.81f),
            new Vector2(1508.611f, 11601.36f),
            new Vector2(1285.232f, 10423.6f),
            new Vector2(1229.664f, 9072.834f),
            new Vector2(1207.795f, 7787.871f),
            new Vector2(1170.487f, 6715.997f),
            new Vector2(1382.255f, 4910.181f),
            new Vector2(1508.93f, 3597.096f),
            new Vector2(1293.969f, 1422.406f)
        };
        private static readonly List<Vector2> RED_BOT_WAYPOINTS = new List<Vector2>
        {
            new Vector2(13719.46f, 12845.7f),
            new Vector2(13253.06f, 11334.05f),
            new Vector2(13364.14f, 10040.18f),
            new Vector2(13696.65f, 8608.403f),
            new Vector2(13710.52f, 7177.168f),
            new Vector2(13741.61f, 5811.374f),
            new Vector2(13751.53f, 4790.221f),
            new Vector2(13529.33f, 3694.875f),
            new Vector2(13001.41f, 2628.15f),
            new Vector2(12294.41f, 1913.186f),
            new Vector2(11432.13f, 1472.96f),
            new Vector2(10506.54f, 1301.828f),
            new Vector2(8870.221f, 1212.785f),
            new Vector2(7975.36f, 1230.857f),
            new Vector2(6925.2f, 1163.12f),
            new Vector2(4844.017f, 1440.128f),
            new Vector2(3471.763f, 1589.286f),
            new Vector2(1293.969f, 1422.406f)
        };
        private static readonly List<Vector2> RED_MID_WAYPOINTS = new List<Vector2>
        {
            new Vector2(12776.26f, 12784.75f),
            new Vector2(11808.24f, 11479),
            new Vector2(10822.95f, 10646.96f),
            new Vector2(10193.23f, 10035.72f),
            new Vector2(9625.553f, 9543.4f),
            new Vector2(9149.958f, 9105.284f),
            new Vector2(8618.293f, 8647.033f),
            new Vector2(8079.619f, 8022.976f),
            new Vector2(7408.699f, 7405.615f),
            new Vector2(6764.715f, 6864.42f),
            new Vector2(6270.211f, 6366.785f),
            new Vector2(5823.8f, 5928.047f),
            new Vector2(5375.718f, 5589.086f),
            new Vector2(4874.661f, 5141.459f),
            new Vector2(3856.273f, 4170.232f),
            new Vector2(2952.614f, 3474.726f),
            new Vector2(1293.969f, 1422.406f)
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
        private static readonly List<MinionSpawnType> DOUBLE_SUPER_MINION_WAVE = new List<MinionSpawnType>
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

        private static readonly Dictionary<TeamId, float[]> _endGameCameraPosition = new Dictionary<TeamId, float[]>
        {
            { TeamId.TEAM_BLUE, new[] { 1551.354f, 1659.627f, 110 } },
            { TeamId.TEAM_PURPLE, new[] { 13243.16f, 13235.05f, 110 } }
        };

        private static readonly Dictionary<TeamId, Target> _spawnsByTeam = new Dictionary<TeamId, Target>
        {
            {TeamId.TEAM_BLUE, new Target(394.7689f, 461.571f)},
            {TeamId.TEAM_PURPLE, new Target(14340.42f, 14391.08f)}
        };

        private static readonly Dictionary<TurretType, int[]> _turretItems = new Dictionary<TurretType, int[]>
        {
            { TurretType.OuterTurret, new[] { 1500, 1501, 1502, 1503 } },
            { TurretType.InnerTurret, new[] { 1500, 1501, 1502, 1503, 1504 } },
            { TurretType.InhibitorTurret, new[] { 1501, 1502, 1503, 1505 } },
            { TurretType.NexusTurret, new[] { 1501, 1502, 1503, 1505 } }
        };

        private Logger _logger = Program.ResolveDependency<Logger>();
        private int _cannonMinionCount;

        public NewSummonersRift(Game game) : base(game, 90 * 1000, 30 * 1000, 90 * 1000, true, 11)
        {
            var path = Path.Combine(
                Program.ExecutingDirectory,
                "Content",
                "Data",
                _game.Config.ContentManager.GameModeName,
                "AIMesh",
                "Map" + GetMapId(),
                "AIPath.aimesh_ngrid"
            );

            if (File.Exists(path))
            {
                NavGrid = NavGridReader.ReadBinary(path);
            }
            else
            {
                _logger.LogCoreError("Failed to load Summoner's Rift data.");
                return;
            }

            AddObject(new LaneTurret("Turret_T1_R_03_A", 10504.25f, 1029.717f, TeamId.TEAM_BLUE, TurretType.OuterTurret, GetTurretItems(TurretType.OuterTurret)));
            AddObject(new LaneTurret("Turret_T1_R_02_A", 6919.156f, 1483.599f, TeamId.TEAM_BLUE, TurretType.InnerTurret, GetTurretItems(TurretType.InnerTurret)));
            AddObject(new LaneTurret("Turret_T1_C_07_A", 4281.712f, 1253.569f, TeamId.TEAM_BLUE, TurretType.InhibitorTurret, GetTurretItems(TurretType.InhibitorTurret)));
            AddObject(new LaneTurret("Turret_T2_R_03_A", 13866.24f, 4505.224f, TeamId.TEAM_PURPLE, TurretType.OuterTurret, GetTurretItems(TurretType.OuterTurret)));
            AddObject(new LaneTurret("Turret_T2_R_02_A", 13327.42f, 8226.276f, TeamId.TEAM_PURPLE, TurretType.InnerTurret, GetTurretItems(TurretType.InnerTurret)));
            AddObject(new LaneTurret("Turret_T2_R_01_A", 13624.75f, 10572.77f, TeamId.TEAM_PURPLE, TurretType.InhibitorTurret, GetTurretItems(TurretType.InhibitorTurret)));
            AddObject(new LaneTurret("Turret_T1_C_05_A", 5846.097f, 6396.75f, TeamId.TEAM_BLUE, TurretType.OuterTurret, GetTurretItems(TurretType.OuterTurret)));
            AddObject(new LaneTurret("Turret_T1_C_04_A", 5048.07f, 4812.894f, TeamId.TEAM_BLUE, TurretType.InnerTurret, GetTurretItems(TurretType.InnerTurret)));
            AddObject(new LaneTurret("Turret_T1_C_03_A", 3651.902f, 3696.424f, TeamId.TEAM_BLUE, TurretType.InhibitorTurret, GetTurretItems(TurretType.InhibitorTurret)));
            AddObject(new LaneTurret("Turret_T1_C_01_A", 1748.261f, 2270.707f, TeamId.TEAM_BLUE, TurretType.NexusTurret, GetTurretItems(TurretType.NexusTurret)));
            AddObject(new LaneTurret("Turret_T1_C_02_A", 2177.64f, 1807.63f, TeamId.TEAM_BLUE, TurretType.NexusTurret, GetTurretItems(TurretType.NexusTurret)));
            AddObject(new LaneTurret("Turret_T2_C_05_A", 8955.434f, 8510.48f, TeamId.TEAM_PURPLE, TurretType.OuterTurret, GetTurretItems(TurretType.OuterTurret)));
            AddObject(new LaneTurret("Turret_T2_C_04_A", 9767.701f, 10113.61f, TeamId.TEAM_PURPLE, TurretType.InnerTurret, GetTurretItems(TurretType.InnerTurret)));
            AddObject(new LaneTurret("Turret_T2_C_03_A", 11134.81f, 11207.94f, TeamId.TEAM_PURPLE, TurretType.InhibitorTurret, GetTurretItems(TurretType.InhibitorTurret)));
            AddObject(new LaneTurret("Turret_T2_C_01_A", 13052.92f, 12612.38f, TeamId.TEAM_PURPLE, TurretType.NexusTurret, GetTurretItems(TurretType.NexusTurret)));
            AddObject(new LaneTurret("Turret_T2_C_02_A", 12611.18f, 13084.11f, TeamId.TEAM_PURPLE, TurretType.NexusTurret, GetTurretItems(TurretType.NexusTurret)));
            AddObject(new LaneTurret("Turret_OrderTurretShrine_A", 105.9285f, 134.494f, TeamId.TEAM_BLUE, TurretType.FountainTurret, GetTurretItems(TurretType.FountainTurret)));
            AddObject(new LaneTurret("Turret_ChaosTurretShrine_A", 14576.36f, 14693.83f, TeamId.TEAM_PURPLE, TurretType.FountainTurret, GetTurretItems(TurretType.FountainTurret)));
            AddObject(new LaneTurret("Turret_T1_L_03_A", 981.2834f, 10441.45f, TeamId.TEAM_BLUE,  TurretType.OuterTurret, _turretItems[TurretType.OuterTurret]));
            AddObject(new LaneTurret("Turret_T1_L_02_A", 1512.892f, 6699.57f, TeamId.TEAM_BLUE, TurretType.InnerTurret, GetTurretItems(TurretType.InnerTurret)));
            AddObject(new LaneTurret("Turret_T1_C_06_A", 1169.962f, 4287.443f, TeamId.TEAM_BLUE, TurretType.InhibitorTurret, GetTurretItems(TurretType.InhibitorTurret)));
            AddObject(new LaneTurret("Turret_T2_L_03_A", 4318.304f, 13875.8f, TeamId.TEAM_PURPLE, TurretType.OuterTurret, _turretItems[TurretType.OuterTurret]));
            AddObject(new LaneTurret("Turret_T2_L_02_A", 7943.152f, 13411.8f, TeamId.TEAM_PURPLE, TurretType.InnerTurret, GetTurretItems(TurretType.InnerTurret)));
            AddObject(new LaneTurret("Turret_T2_L_01_A", 10481.09f, 13650.54f, TeamId.TEAM_PURPLE, TurretType.InhibitorTurret, GetTurretItems(TurretType.InhibitorTurret)));

            //TODO
            const int COLLISION_RADIUS = 0;
            const int SIGHT_RANGE = 1700;

            AddObject(new Inhibitor("SRUAP_OrderInhibitor", TeamId.TEAM_BLUE, COLLISION_RADIUS, 1171.828f, 3571.784f, SIGHT_RANGE, 0xffd23c3e)); //top
            AddObject(new Inhibitor("SRUAP_OrderInhibitor", TeamId.TEAM_BLUE, COLLISION_RADIUS, 3203.029f, 3208.784f, SIGHT_RANGE, 0xff4a20f1)); //mid
            AddObject(new Inhibitor("SRUAP_OrderInhibitor", TeamId.TEAM_BLUE, COLLISION_RADIUS, 3452.529f, 1236.884f, SIGHT_RANGE, 0xff9303e1)); //bot
            AddObject(new Inhibitor("SRUAP_ChaosInhibitor", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 11261.67f, 13676.56f, SIGHT_RANGE, 0xff6793d0)); //top
            AddObject(new Inhibitor("SRUAP_ChaosInhibitor", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 11598.12f, 11667.81f, SIGHT_RANGE, 0xffff8f1f)); //mid
            AddObject(new Inhibitor("SRUAP_ChaosInhibitor", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 13604.6f, 11316.01f, SIGHT_RANGE, 0xff26ac0f)); //bot

            AddObject(new Nexus("SRUAP_OrderNexus", TeamId.TEAM_BLUE, COLLISION_RADIUS, 1551.354f, 1659.627f, SIGHT_RANGE, 0xfff97db5));
            AddObject(new Nexus("SRUAP_ChaosNexus", TeamId.TEAM_PURPLE, COLLISION_RADIUS, 13243.16f, 13235.05f, SIGHT_RANGE, 0xfff02c0f));

            AddObject(new LevelProp(5194.136f, 123.4686f, 7012.206f, 0f, -118.7245f, 0f, 0f, 0f, "LevelProp_SRU_AntlerMouse", "Sru_Antlermouse"));
            AddObject(new LevelProp(2213.773f, 166.21f, 6963.584f, -41.06179f, 71.1862f, -27.87154f, 0f, 0f, "LevelProp_SRU_AntlerMouse1", "Sru_Antlermouse"));
            AddObject(new LevelProp(1888.702f, 95.09792f, 4102.505f, -179.673f, 61.95893f, 178.3296f, 0f, 0f, "LevelProp_SRU_AntlerMouse10", "Sru_Antlermouse"));
            AddObject(new LevelProp(4220.001f, 43.72308f, 4499.094f, 0f, -38.21672f, 0f, 0f, 0f, "LevelProp_SRU_AntlerMouse11", "Sru_Antlermouse"));
            AddObject(new LevelProp(2894.08f, 44.89213f, 5653.996f, 0f, -116.5409f, 0f, 0f, 0f, "LevelProp_SRU_AntlerMouse2", "Sru_Antlermouse"));
            AddObject(new LevelProp(3803.946f, 45.4187f, 6503.979f, 0f, -170.5758f, 0f, 0f, 0f, "LevelProp_SRU_AntlerMouse3", "Sru_Antlermouse"));
            AddObject(new LevelProp(4882.856f, 46.1818f, 6505.081f, 0f, -142.632f, 0f, 0f, 0f, "LevelProp_SRU_AntlerMouse4", "Sru_Antlermouse"));
            AddObject(new LevelProp(2909.064f, 47.40145f, 7627.799f, -179.2892f, 52.327f, 177.9687f, 0f, 0f, "LevelProp_SRU_AntlerMouse5", "Sru_Antlermouse"));
            AddObject(new LevelProp(2054.266f, 50.50199f, 5703.996f, -11.7858f, -191.3913f, 3.627099f, 0f, 0f, "LevelProp_SRU_AntlerMouse6", "Sru_Antlermouse"));
            AddObject(new LevelProp(2347.906f, 182.9949f, 10639.1f, -173.2344f, 65.05644f, -175.3394f, 0f, 0f, "LevelProp_SRU_AntlerMouse7", "Sru_Antlermouse"));
            AddObject(new LevelProp(2998.508f, 100.9301f, 8743.376f, -2.580484f, 30.51813f, -8.916169f, 0f, 0f, "LevelProp_SRU_AntlerMouse8", "Sru_Antlermouse"));
            AddObject(new LevelProp(1984.291f, 94.60245f, 4133.518f, 0f, -158.0521f, 0f, 0f, 0f, "LevelProp_SRU_AntlerMouse9", "Sru_Antlermouse"));
            AddObject(new LevelProp(12418.92f, 141.6267f, 4051.257f, 0f, -199.9286f, 0f, 0f, 0f, "LevelProp_sru_bird", "SRU_Bird"));
            AddObject(new LevelProp(1507.258f, 413.5416f, -308.4872f, 1.433609f, 46.41291f, 6.451541f, 0f, 0f, "LevelProp_sru_bird1", "SRU_Bird"));
            AddObject(new LevelProp(147.655f, 453.4493f, 8168.214f, -8.918667f, 76.55724f, -7.190923f, 0f, 0f, "LevelProp_sru_bird2", "SRU_Bird"));
            AddObject(new LevelProp(14391.08f, 375.6806f, 3836.75f, -168.1189f, -51.90569f, 169.531f, 0f, 0f, "LevelProp_sru_bird4", "SRU_Bird"));
            AddObject(new LevelProp(8080.158f, 468.2289f, 198.3275f, 22.96237f, 52.54985f, 16.68683f, 0f, 0f, "LevelProp_sru_bird5", "SRU_Bird"));
            AddObject(new LevelProp(-7058.438f, -10384.66f, 25191.99f, 0f, 87.20969f, 0f, 0f, 0f, "LevelProp_sru_dragon_prop", "sru_dragon_prop"));
            AddObject(new LevelProp(-4157.41f, -5639.87f, 2518.217f, 0f, 1.310995f, 0f, 0f, 0f, "LevelProp_sru_dragon_prop1", "sru_dragon_prop"));
            AddObject(new LevelProp(9942.157f, 45.9148f, 8358.395f, -179.2027f, -8.776432f, -179.9072f, 0f, 0f, "LevelProp_sru_gromp_prop", "Sru_Gromp_Prop"));
            AddObject(new LevelProp(11801.39f, 106.3826f, 6318.263f, -175.8791f, -13.76043f, 176.8972f, 0f, 0f, "LevelProp_sru_gromp_prop10", "Sru_Gromp_Prop"));
            AddObject(new LevelProp(14301.71f, 92.15693f, 11051.72f, -179.5595f, 3.120555f, -179.9858f, 0f, 0f, "LevelProp_sru_gromp_prop11", "Sru_Gromp_Prop"));
            AddObject(new LevelProp(12425.78f, 94.93915f, 10906.08f, -15.83221f, -151.3309f, 3.553761f, 0f, 0f, "LevelProp_sru_gromp_prop12", "Sru_Gromp_Prop"));
            AddObject(new LevelProp(14100.43f, 46.98303f, 6161.51f, 179.8837f, 51.78839f, 179.2859f, 0f, 0f, "LevelProp_sru_gromp_prop13", "Sru_Gromp_Prop"));
            AddObject(new LevelProp(10567.48f, 50.95081f, 9201.183f, -175.7276f, 5.590603f, -179.3657f, 0f, 0f, "LevelProp_sru_gromp_prop3", "Sru_Gromp_Prop"));
            AddObject(new LevelProp(9452.197f, 214.4514f, 7685.849f, -1.901987f, -153.9183f, 2.83302f, 0f, 0f, "LevelProp_sru_gromp_prop5", "Sru_Gromp_Prop"));
            AddObject(new LevelProp(11746.37f, 43.98136f, 9005.245f, -176.5635f, 21.18202f, 180f, 0f, 0f, "LevelProp_sru_gromp_prop6", "Sru_Gromp_Prop"));
            AddObject(new LevelProp(12898.96f, 43.92405f, 5151.044f, 0f, -111.2844f, 0f, 0f, 0f, "LevelProp_sru_gromp_prop7", "Sru_Gromp_Prop"));
            AddObject(new LevelProp(11793.68f, 43.78664f, 7372.605f, -0.7624031f, -145.2645f, 2.086306f, 0f, 0f, "LevelProp_sru_gromp_prop8", "Sru_Gromp_Prop"));
            AddObject(new LevelProp(9394.181f, -73.8051f, 6374.51f, 0f, -265.1839f, 0f, 0f, 0f, "LevelProp_sru_gromp_prop9", "Sru_Gromp_Prop"));
            AddObject(new LevelProp(8090.797f, 186.2086f, 11305.53f, -21.22f, -67.87017f, 6.758901f, 0f, 0f, "LevelProp_sru_lizard", "Sru_Lizard"));
            AddObject(new LevelProp(7079.771f, 232.6451f, 12051.46f, -14.74398f, 66.61549f, -15.23072f, 0f, 0f, "LevelProp_sru_lizard1", "Sru_Lizard"));
            AddObject(new LevelProp(10761.94f, 262.4519f, 14473.77f, -96.73441f, -67.73241f, 97.87612f, 0f, 0f, "LevelProp_sru_lizard10", "Sru_Lizard"));
            AddObject(new LevelProp(9966.531f, 255.328f, 12602.97f, 0f, 0f, -20.647f, 0f, 0f, "LevelProp_sru_lizard11", "Sru_Lizard"));
            AddObject(new LevelProp(7564.231f, 367.1433f, 14262.36f, -78.48996f, -78.23854f, 78.18267f, 0f, 0f, "LevelProp_sru_lizard12", "Sru_Lizard"));
            AddObject(new LevelProp(9426.188f, 134.3351f, 10994.1f, 84.66563f, 62.78813f, 88.71596f, 0f, 0f, "LevelProp_sru_lizard2", "Sru_Lizard"));
            AddObject(new LevelProp(6678.113f, 224.0654f, 11199.31f, -165.3242f, -29.73f, 171.8765f, 0f, 0f, "LevelProp_sru_lizard3", "Sru_Lizard"));
            AddObject(new LevelProp(8395.145f, 157.3293f, 10579.15f, -6.706638f, 29.38646f, -8.965001f, 0f, 0f, "LevelProp_sru_lizard4", "Sru_Lizard"));
            AddObject(new LevelProp(9018.509f, 106.8651f, 12353.65f, -0.3097325f, -153.5943f, 1.015439f, 0f, 0f, "LevelProp_sru_lizard6", "Sru_Lizard"));
            AddObject(new LevelProp(7299.199f, 180.2996f, 9521.847f, -74.57303f, -77.99239f, 84.01626f, 0f, 0f, "LevelProp_sru_lizard7", "Sru_Lizard"));
            AddObject(new LevelProp(4882.003f, 209.6273f, 12030.8f, 7.925711f, 48.34449f, 24.48676f, 0f, 0f, "LevelProp_sru_lizard8", "Sru_Lizard"));
            AddObject(new LevelProp(10844.38f, 168.5582f, 12488.78f, -14.24151f, 9.904963f, 27.80418f, 0f, 0f, "LevelProp_sru_lizard9", "Sru_Lizard"));
            AddObject(new LevelProp(5559.504f, 101.4287f, 3932.01f, -162.4397f, 80.55253f, -160.883f, 0f, 0f, "LevelProp_sru_snail", "Sru_Snail"));
            AddObject(new LevelProp(6533.019f, 199.9412f, 2319.846f, 4.041014f, -20.57683f, 8.526711f, 0f, 0f, "LevelProp_sru_snail1", "Sru_Snail"));
            AddObject(new LevelProp(9426.731f, 206.1233f, 2943.642f, -40.76818f, -93.64982f, 12.20924f, 0f, 0f, "LevelProp_sru_snail3", "Sru_Snail"));
            AddObject(new LevelProp(5958.451f, 140.5533f, 4784.142f, -177.7563f, -78.35416f, 178.4867f, 0f, 0f, "LevelProp_sru_snail4", "Sru_Snail"));
            AddObject(new LevelProp(6417.55f, 46.29026f, 3235.403f, -178.0568f, 59.60561f, 180f, 0f, 0f, "LevelProp_sru_snail5", "Sru_Snail"));
            AddObject(new LevelProp(10269.67f, 74.85492f, 3351.237f, -22.33266f, -106.3702f, 18.6251f, 0f, 0f, "LevelProp_sru_snail6", "Sru_Snail"));
            AddObject(new LevelProp(4701.505f, 97.24113f, 770.3691f, -177.427f, -6.151691f, 179.9008f, 0f, 0f, "LevelProp_sru_snail7", "Sru_Snail"));
            AddObject(new LevelProp(392.3917f, 122.835f, 1311.946f, 176.7851f, 88.51321f, 167.4041f, 0f, 0f, "LevelProp_sru_snail8", "Sru_Snail"));
            AddObject(new LevelProp(4141.109f, 97.97998f, 2237.708f, -122.4207f, -82.60841f, 122.5098f, 0f, 0f, "LevelProp_sru_snail9", "Sru_Snail"));
            AddObject(new LevelProp(13727.21f, 144.6702f, 14592.51f, 0f, 237.5103f, 0f, 0f, 0f, "LevelProp_SRU_storeKeeperNorth", "sru_storekeepernorth"));
            AddObject(new LevelProp(40.03181f, 162.7097f, 1112.402f, 0f, 135f, 0f, 0f, 0f, "LevelProp_sru_storekeepersouth", "sru_storekeepersouth"));

            // Start at xp to reach level 1
            ExpToLevelUp = new List<int>
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

            // Set first minion spawn and first gold time to be 1:30
            _firstSpawnTime = 90 * 1000;
            FirstGoldTime = _firstSpawnTime;

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
            {
                return null;
            }

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

        public override string GetMinionModel(TeamId team, MinionSpawnType type)
        {
            var toRet = "SRU_";
            switch (team)
            {
                case TeamId.TEAM_BLUE:
                    toRet += "OrderMinion";
                    break;
                case TeamId.TEAM_PURPLE:
                    toRet += "ChaosMinion";
                    break;
            }

            switch (type)
            {
                case MinionSpawnType.MINION_TYPE_CANNON:
                    toRet += "Siege";
                    break;
                case MinionSpawnType.MINION_TYPE_CASTER:
                    toRet += "Ranged";
                    break;
                case MinionSpawnType.MINION_TYPE_MELEE:
                    toRet += "Melee";
                    break;
                case MinionSpawnType.MINION_TYPE_SUPER:
                    toRet += "Super";
                    break;
            }

            return toRet;
        }

        public override float GetGoldPerSecond()
        {
            return 1.9f;
        }

        public override Target GetRespawnLocation(TeamId team)
        {
            if (!_spawnsByTeam.ContainsKey(team))
            {
                return new Target(25.90f, 280);
            }

            return _spawnsByTeam[team];
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
                    float firstDeathGold = gold - gold * 0.085f;

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

            var dic = new Dictionary<MinionSpawnType, float>
            {
                { MinionSpawnType.MINION_TYPE_MELEE, 19.8f + 0.2f * (int)(GameTime / (90 * 1000)) },
                { MinionSpawnType.MINION_TYPE_CASTER, 16.8f + 0.2f * (int)(GameTime / (90 * 1000)) },
                { MinionSpawnType.MINION_TYPE_CANNON, 40.0f + 0.5f * (int)(GameTime / (90 * 1000)) },
                { MinionSpawnType.MINION_TYPE_SUPER, 40.0f + 1.0f * (int)(GameTime / (180 * 1000)) }
            };

            if (!dic.ContainsKey(m.getType()))
            {
                return 0.0f;
            }

            return dic[m.getType()];
        }

        public override float GetExperienceFor(Unit u)
        {
            var m = u as Minion;

            if (m == null)
                return 0.0f;
            var dic = new Dictionary<MinionSpawnType, float>
            {
                { MinionSpawnType.MINION_TYPE_MELEE, 64.0f },
                { MinionSpawnType.MINION_TYPE_CASTER, 32.0f },
                { MinionSpawnType.MINION_TYPE_CANNON, 92.0f },
                { MinionSpawnType.MINION_TYPE_SUPER, 97.0f }
            };

            if (!dic.ContainsKey(m.getType()))
            {
                return 0.0f;
            }

            return dic[m.getType()];
        }

        public override Tuple<TeamId, Vector2> GetMinionSpawnPosition(MinionSpawnPosition spawnPosition)
        {
            switch (spawnPosition)
            {
                case MinionSpawnPosition.SPAWN_BLUE_TOP:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_BLUE, new Vector2(1109.416f, 2091.567f));
                case MinionSpawnPosition.SPAWN_BLUE_BOT:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_BLUE, new Vector2(2034.091f, 1171.083f));
                case MinionSpawnPosition.SPAWN_BLUE_MID:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_BLUE, new Vector2(2008.126f, 2079.599f));
                case MinionSpawnPosition.SPAWN_RED_TOP:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_PURPLE, new Vector2(12800.61f, 13745.36f));
                case MinionSpawnPosition.SPAWN_RED_BOT:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_PURPLE, new Vector2(13719.46f, 12845.7f));
                case MinionSpawnPosition.SPAWN_RED_MID:
                    return new Tuple<TeamId, Vector2>(TeamId.TEAM_PURPLE, new Vector2(12776.26f, 12784.75f));
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
                {MinionSpawnPosition.SPAWN_BLUE_BOT, Tuple.Create(BLUE_BOT_WAYPOINTS, 0xff26ac0f)},
                {MinionSpawnPosition.SPAWN_BLUE_MID, Tuple.Create(BLUE_MID_WAYPOINTS, 0xffff8f1f)},
                {MinionSpawnPosition.SPAWN_BLUE_TOP, Tuple.Create(BLUE_TOP_WAYPOINTS, 0xff6793d0)},
                {MinionSpawnPosition.SPAWN_RED_BOT, Tuple.Create(RED_BOT_WAYPOINTS, 0xff9303e1)},
                {MinionSpawnPosition.SPAWN_RED_MID, Tuple.Create(RED_MID_WAYPOINTS, 0xff4a20f1)},
                {MinionSpawnPosition.SPAWN_RED_TOP, Tuple.Create(RED_TOP_WAYPOINTS, 0xffd23c3e)}
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

                var oppositeTeam = TeamId.TEAM_BLUE;
                if (inhibitor.Team == TeamId.TEAM_PURPLE)
                {
                    oppositeTeam = TeamId.TEAM_PURPLE;
                }

                var areAllInhibitorsDead = AllInhibitorsDestroyedFromTeam(oppositeTeam) && !inhibitor.RespawnAnnounced;

                var list = REGULAR_MINION_WAVE;
                if (_cannonMinionCount >= cannonMinionCap)
                {
                    list = CANNON_MINION_WAVE;
                }

                if (isInhibitorDead)
                {
                    list = SUPER_MINION_WAVE;
                }

                if (areAllInhibitorsDead)
                {
                    list = DOUBLE_SUPER_MINION_WAVE;
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
            return 11;
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
