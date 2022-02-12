using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Maps;
using LeagueSandbox.GameServer.API;
using System.Collections.Generic;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static GameServerLib.API.APIMapFunctionManager;

namespace MapScripts.Map8
{
    public class NeutralMinionSpawn
    {
        private static bool crystalSpawned;
        private static bool forceSpawn;

        static List<IMinion> InfoPoints = new List<IMinion>();
        static List<IMonsterCamp> SpeedShrines = new List<IMonsterCamp>();
        static List<IMonsterCamp> HealthPacks = new List<IMonsterCamp>();

        //Since the center crystals are treated as simple minions intead of camp/monster, we have to hand everything individually
        static Dictionary<TeamId, IMinion> Crystals = new Dictionary<TeamId, IMinion>();
        static List<MinionTemplate> CrystalsTemplates = new List<MinionTemplate>();
        static Dictionary<TeamId, float> CrystalTimers = new Dictionary<TeamId, float> { { TeamId.TEAM_BLUE, 180.0f * 1000 }, { TeamId.TEAM_PURPLE, 180.0f * 1000 } };
        static Dictionary<TeamId, List<IRegion>> CrystalRegions = new Dictionary<TeamId, List<IRegion>> { { TeamId.TEAM_BLUE, new List<IRegion>() }, { TeamId.TEAM_PURPLE, new List<IRegion>() } };
        public static void InitializeNeutrals(IMapScriptHandler map)
        {
            for (int i = 0; i < map.InfoPoints.Count; i++)
            {
                var point = CreateMinion("OdinNeutralGuardian", "OdinNeutralGuardian", new Vector2(map.InfoPoints[i].CentralPoint.X, map.InfoPoints[i].CentralPoint.Z), ignoreCollision: true);
                InfoPoints.Add(point);
                AddUnitPerceptionBubble(point, 800.0f, 25000.0f, TeamId.TEAM_BLUE, true, collisionArea: 120.0f, collisionOwner: point);
                InfoPoints[i].PauseAi(true);
            }

            CrystalsTemplates.Add(new MinionTemplate(null, "OdinCenterRelic", "OdinCenterRelic", new Vector2(7074.9736f, 6462.0273f), team: TeamId.TEAM_BLUE));
            CrystalsTemplates.Add(new MinionTemplate(null, "OdinCenterRelic", "OdinCenterRelic", new Vector2(6801.1855f, 6462.0273f), team: TeamId.TEAM_PURPLE));

            SetupCamps();

            foreach (var camp in SpeedShrines)
            {
                if (!camp.IsAlive)
                {
                    AddPosPerceptionBubble(new Vector2(camp.Position.X, camp.Position.Z), 250.0f, 1.0f, TeamId.TEAM_BLUE);
                    AddPosPerceptionBubble(new Vector2(camp.Position.X, camp.Position.Z), 250.0f, 1.0f, TeamId.TEAM_PURPLE);
                    SpawnCamp(camp);
                }
            }
        }

        public static void OnUpdate(float diff)
        {
            if (crystalSpawned)
            {
                foreach (var crystal in Crystals.Values)
                {
                    string iconCategory = "CenterRelicLeft";

                    if (crystal.Team == TeamId.TEAM_PURPLE)
                    {
                        iconCategory = "CenterRelicRight";
                    }

                    //For some Reason this only works here
                    SetMinimapIcon(crystal, iconCategory, true);
                }

                crystalSpawned = false;
            }

            foreach (var camp in HealthPacks)
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

            foreach (var crystalTemplate in CrystalsTemplates)
            {
                if (!Crystals.ContainsKey(crystalTemplate.Team))
                {
                    CrystalTimers[crystalTemplate.Team] -= diff;

                    if (CrystalTimers[crystalTemplate.Team] <= 0 || forceSpawn)
                    {
                        var crystal = CreateMinion(crystalTemplate.Name, crystalTemplate.Model, crystalTemplate.Position,
                                crystalTemplate.NetId, crystalTemplate.Team, crystalTemplate.SkinId,
                                crystalTemplate.IgnoresCollision, crystalTemplate.IsTargetable);

                        AddUnitPerceptionBubble(crystal, 350.0f, 25000.0f, TeamId.TEAM_BLUE, collisionArea: 38.08f, collisionOwner: crystal);
                        AddUnitPerceptionBubble(crystal, 350.0f, 25000.0f, TeamId.TEAM_PURPLE, collisionArea: 38.08f, collisionOwner: crystal);

                        ApiEventManager.OnDeath.AddListener(crystal, crystal, OnCrystalDeath, true);

                        Crystals.Add(crystal.Team, crystal);
                        CrystalTimers[crystalTemplate.Team] = 180.0f * 1000f;

                        crystalSpawned = true;
                    }
                }
            }
            if (forceSpawn)
            {
                forceSpawn = false;
            }
        }

        public static void OnCrystalDeath(IDeathData deathData)
        {
            Crystals.Remove(deathData.Unit.Team);
            foreach (var region in CrystalRegions[deathData.Unit.Team])
            {
                region.SetToRemove();
            }
        }

        public static void ForceCampSpawn()
        {
            forceSpawn = true;
        }

        public static void SetupCamps()
        {
            var speedShrine1 = CreateJungleCamp(new Vector3(5022.9287f, 60.0f, 7778.2695f), 102, 0, "Shrine", 0);
            CreateJungleMonster("OdinSpeedShrine", "OdinSpeedShrine", new Vector2(5022.9287f, 7778.2695f), new Vector3(-0.0f, 0.0f, 1.0f), speedShrine1, isTargetable: false, ignoresCollision: true);
            SpeedShrines.Add(speedShrine1);

            var speedShrine2 = CreateJungleCamp(new Vector3(8859.897f, 60.0f, 7788.1064f), 103, 0, "Shrine", 0);
            CreateJungleMonster("OdinSpeedShrine", "OdinSpeedShrine", new Vector2(8859.897f, 7788.1064f), new Vector3(-0.0f, 0.0f, 1.0f), speedShrine2, isTargetable: false, ignoresCollision: true);
            SpeedShrines.Add(speedShrine2);

            var speedShrine3 = CreateJungleCamp(new Vector3(6962.6934f, 60.0f, 4089.48f), 104, 0, "Shrine", 0);
            CreateJungleMonster("OdinSpeedShrine", "OdinSpeedShrine", new Vector2(6962.6934f, 4089.48f), new Vector3(-0.0f, 0.0f, 1.0f), speedShrine3, isTargetable: false, ignoresCollision: true);
            SpeedShrines.Add(speedShrine3);


            var healthPacket1 = CreateJungleCamp(new Vector3(4948.231f, 60.0f, 9329.905f), 100, 0, "HealthPack", 120.0f * 1000f);
            CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(4948.231f, 9329.905f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket1);
            HealthPacks.Add(healthPacket1);

            var healthPacket2 = CreateJungleCamp(new Vector3(8972.231f, 60.0f, 9329.905f), 101, 0, "HealthPack", 120.0f * 1000f);
            CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(8972.231f, 9329.905f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket2);
            HealthPacks.Add(healthPacket2);

            var healthPacket3 = CreateJungleCamp(new Vector3(6949.8193f, 60.0f, 2855.0513f), 112, 0, "HealthPack", 120.0f * 1000f);
            CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(6949.8193f, 2855.0513f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket3);
            HealthPacks.Add(healthPacket3);

            var healthPacket4 = CreateJungleCamp(new Vector3(6947.838f, 60.0f, 12116.367f), 108, 0, "HealthPack", 120.0f * 1000f);
            CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(6947.838f, 12116.367f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket4);
            HealthPacks.Add(healthPacket4);

            var healthPacket5 = CreateJungleCamp(new Vector3(12881.534f, 60.0f, 8294.764f), 109, 0, "HealthPack", 120.0f * 1000f);
            CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(12881.534f, 8294.764f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket5);
            HealthPacks.Add(healthPacket5);

            var healthPacket6 = CreateJungleCamp(new Vector3(10242.127f, 60.0f, 1519.5938f), 105, 0, "HealthPack", 120.0f * 1000f);
            CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(10242.127f, 1519.5938f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket6);
            HealthPacks.Add(healthPacket6);

            var healthPacket7 = CreateJungleCamp(new Vector3(3639.7327f, 60.0f, 1490.0762f), 106, 0, "HealthPack", 120.0f * 1000f);
            CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(3639.7327f, 1490.0762f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket7);
            HealthPacks.Add(healthPacket7);

            var healthPacket8 = CreateJungleCamp(new Vector3(1027.4365f, 60.0f, 8288.714f), 107, 0, "HealthPack", 120.0f * 1000f);
            CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(1027.4365f, 8288.714f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket8);
            HealthPacks.Add(healthPacket8);

            var healthPacket9 = CreateJungleCamp(new Vector3(4324.928f, 60.0f, 5500.919f), 110, 0, "HealthPack", 120.0f * 1000f);
            CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(4324.928f, 5500.919f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket9);
            HealthPacks.Add(healthPacket9);

            var healthPacket10 = CreateJungleCamp(new Vector3(9573.432f, 60.0f, 5530.13f), 111, 0, "HealthPack", 120.0f * 1000f);
            CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(9573.432f, 5530.13f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket10);
            HealthPacks.Add(healthPacket10);
        }
    }
    public class MinionTemplate
    {
        public IObjAiBase Owner { get; set; }
        public string Name { get; set; }
        public string Model { get; set; }
        public Vector2 Position { get; set; }
        public int SkinId { get; set; }
        public TeamId Team { get; set; }
        public uint NetId { get; set; }
        public bool IsTargetable { get; set; }
        public bool IgnoresCollision { get; set; }
        public string AiScript { get; set; }
        public int DamageBonus { get; set; }
        public int HealthBonus { get; set; }
        public int InitialLevel { get; set; }
        public IObjAiBase VisibilityOwner { get; set; }

        public MinionTemplate(
            IObjAiBase owner,
            string model,
            string name,
            Vector2 position,
            uint netId = 0,
            TeamId team = TeamId.TEAM_NEUTRAL,
            int skinId = 0,
            bool ignoreCollision = false,
            bool targetable = true,
            IObjAiBase visibilityOwner = null,
            string aiScript = "",
            int damageBonus = 0,
            int healthBonus = 0,
            int initialLevel = 1
        )
        {
            Owner = owner;
            Name = name;
            Model = model;
            Team = team;
            Position = position;
            NetId = netId;
            IsTargetable = targetable;
            IgnoresCollision = ignoreCollision;
            AiScript = aiScript;
            DamageBonus = damageBonus;
            HealthBonus = healthBonus;
            InitialLevel = initialLevel;
            VisibilityOwner = visibilityOwner;
            SkinId = skinId;
        }
    }
}
