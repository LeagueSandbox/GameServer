using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using System.Collections.Generic;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;

namespace MapScripts.Map8
{
    public class NeutralMinionSpawn
    {
        private static bool forceSpawn;

        static Dictionary<IMonsterCamp, IMonster> SpeedShrines = new Dictionary<IMonsterCamp, IMonster>();
        static Dictionary<IMonsterCamp, IMonster> HealthPacks = new Dictionary<IMonsterCamp, IMonster>();
        static List<Crystal> Crystals = new List<Crystal>();

        public static void InitializeNeutrals()
        {
            SetupCamps();

            Crystals.Add(new Crystal(new Vector2(7074.9736f, 6462.0273f), TeamId.TEAM_BLUE));
            Crystals.Add(new Crystal(new Vector2(6801.1855f, 6462.0273f), TeamId.TEAM_PURPLE));

            foreach (var camp in SpeedShrines.Keys)
            {
                if (!camp.IsAlive)
                {
                    AddPosPerceptionBubble(new Vector2(camp.Position.X, camp.Position.Z), 250.0f, 1.0f, TeamId.TEAM_BLUE);
                    AddPosPerceptionBubble(new Vector2(camp.Position.X, camp.Position.Z), 250.0f, 1.0f, TeamId.TEAM_PURPLE);
                    camp.AddMonster(SpeedShrines[camp]);
                }
            }
        }

        public static void OnUpdate(float diff)
        {
            foreach (var camp in HealthPacks.Keys)
            {
                if (!camp.IsAlive)
                {
                    camp.RespawnTimer -= diff;
                    if (camp.RespawnTimer <= 0 || forceSpawn)
                    {
                        SpawnCamp(camp);
                        camp.RespawnTimer = 30.0f * 1000f;
                    }
                }
            }

            foreach (var crystal in Crystals)
            {
                if (crystal.IsDead)
                {
                    crystal.RespawnTimer -= diff;
                    if (crystal.RespawnTimer <= 0 || forceSpawn)
                    {
                        crystal.SpawnCrystal();
                        crystal.RespawnTimer = 180.0f * 1000f;
                    }
                }
            }
            if (forceSpawn)
            {
                forceSpawn = false;
            }
        }

        public static void SpawnCamp(IMonsterCamp monsterCamp)
        {
            monsterCamp.AddMonster(HealthPacks[monsterCamp]);
        }

        public static void ForceCampSpawn()
        {
            forceSpawn = true;
        }

        public static void SetupCamps()
        {
            var speedShrine1 = CreateJungleCamp(new Vector3(5022.9287f, 60.0f, 7778.2695f), 102, 0, "Shrine", 0);
            SpeedShrines.Add(speedShrine1, CreateJungleMonster("OdinSpeedShrine", "OdinSpeedShrine", new Vector2(5022.9287f, 7778.2695f), new Vector3(-0.0f, 0.0f, 1.0f), speedShrine1, isTargetable: false, ignoresCollision: true));

            var speedShrine2 = CreateJungleCamp(new Vector3(8859.897f, 60.0f, 7788.1064f), 103, 0, "Shrine", 0);
            SpeedShrines.Add(speedShrine2, CreateJungleMonster("OdinSpeedShrine", "OdinSpeedShrine", new Vector2(8859.897f, 7788.1064f), new Vector3(-0.0f, 0.0f, 1.0f), speedShrine2, isTargetable: false, ignoresCollision: true));

            var speedShrine3 = CreateJungleCamp(new Vector3(6962.6934f, 60.0f, 4089.48f), 104, 0, "Shrine", 0);
            SpeedShrines.Add(speedShrine3, CreateJungleMonster("OdinSpeedShrine", "OdinSpeedShrine", new Vector2(6962.6934f, 4089.48f), new Vector3(-0.0f, 0.0f, 1.0f), speedShrine3, isTargetable: false, ignoresCollision: true));


            var healthPacket1 = CreateJungleCamp(new Vector3(4948.231f, 60.0f, 9329.905f), 100, 0, "HealthPack", 120.0f * 1000f);
            HealthPacks.Add(healthPacket1, CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(4948.231f, 9329.905f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket1));

            var healthPacket2 = CreateJungleCamp(new Vector3(8972.231f, 60.0f, 9329.905f), 101, 0, "HealthPack", 120.0f * 1000f);
            HealthPacks.Add(healthPacket2, CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(8972.231f, 9329.905f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket2));

            var healthPacket3 = CreateJungleCamp(new Vector3(6949.8193f, 60.0f, 2855.0513f), 112, 0, "HealthPack", 120.0f * 1000f);
            HealthPacks.Add(healthPacket3, CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(6949.8193f, 2855.0513f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket3));

            var healthPacket4 = CreateJungleCamp(new Vector3(6947.838f, 60.0f, 12116.367f), 108, 0, "HealthPack", 120.0f * 1000f);
            HealthPacks.Add(healthPacket4, CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(6947.838f, 12116.367f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket4));

            var healthPacket5 = CreateJungleCamp(new Vector3(12881.534f, 60.0f, 8294.764f), 109, 0, "HealthPack", 120.0f * 1000f);
            HealthPacks.Add(healthPacket5, CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(12881.534f, 8294.764f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket5));

            var healthPacket6 = CreateJungleCamp(new Vector3(10242.127f, 60.0f, 1519.5938f), 105, 0, "HealthPack", 120.0f * 1000f);
            HealthPacks.Add(healthPacket6, CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(10242.127f, 1519.5938f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket6));

            var healthPacket7 = CreateJungleCamp(new Vector3(3639.7327f, 60.0f, 1490.0762f), 106, 0, "HealthPack", 120.0f * 1000f);
            HealthPacks.Add(healthPacket7, CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(3639.7327f, 1490.0762f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket7));

            var healthPacket8 = CreateJungleCamp(new Vector3(1027.4365f, 60.0f, 8288.714f), 107, 0, "HealthPack", 120.0f * 1000f);
            HealthPacks.Add(healthPacket8, CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(1027.4365f, 8288.714f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket8));

            var healthPacket9 = CreateJungleCamp(new Vector3(4324.928f, 60.0f, 5500.919f), 110, 0, "HealthPack", 120.0f * 1000f);
            HealthPacks.Add(healthPacket9, CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(4324.928f, 5500.919f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket9));

            var healthPacket10 = CreateJungleCamp(new Vector3(9573.432f, 60.0f, 5530.13f), 111, 0, "HealthPack", 120.0f * 1000f);
            HealthPacks.Add(healthPacket10, CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(9573.432f, 5530.13f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket10));
        }
    }

    public class Crystal
    {
        public float RespawnTimer = 180.0f * 1000;
        public bool IsDead = true;

        Vector2 Position;
        TeamId Team;
        List<IRegion> Regions = new List<IRegion>();
        public Crystal(Vector2 position, TeamId team)
        {
            Position = position;
            Team = team;
        }

        public void SpawnCrystal()
        {
            var crystal = CreateMinion("OdinCenterRelic", "OdinCenterRelic", Position, team: Team);

            Regions.Add(AddUnitPerceptionBubble(crystal, 350.0f, 25000.0f, TeamId.TEAM_BLUE, collisionArea: 38.08f, collisionOwner: crystal));
            Regions.Add(AddUnitPerceptionBubble(crystal, 350.0f, 25000.0f, TeamId.TEAM_PURPLE, collisionArea: 38.08f, collisionOwner: crystal));

            ApiEventManager.OnDeath.AddListener(crystal, crystal, OnCrystalDeath, true);
            IsDead = false;
        }

        public void OnCrystalDeath(IDeathData deathData)
        {
            IsDead = true;
            foreach (var region in Regions)
            {
                region.SetToRemove();
            }
        }
    }
}
