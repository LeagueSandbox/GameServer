﻿using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using System.Collections.Generic;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;

namespace MapScripts.Map8
{
    public class NeutralMinionSpawnAscension
    {
        private static bool forceSpawn;

        static Dictionary<IMonsterCamp, IMonster> SpeedShrines = new Dictionary<IMonsterCamp, IMonster>();
        static Dictionary<IMonsterCamp, IMonster> HealthPacks = new Dictionary<IMonsterCamp, IMonster>();
        static List<AscensionCrystal> CaptureCrystals = new List<AscensionCrystal>();
        static AscXerath Xerath;

        //Since the center crystals are treated as simple minions intead of camp/monster, we have to hand everything individually
        public static void InitializeNeutrals()
        {
            SetupCamps();

            foreach (var camp in SpeedShrines.Keys)
            {
                camp.AddMonster(SpeedShrines[camp]);
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
                        camp.RespawnTimer = 20.0f * 1000f;
                    }
                }
            }

            if (!Xerath.Camp.IsAlive)
            {
                Xerath.RespawnTimer -= diff;
                if(Xerath.RespawnTimer <= 0)
                {
                    Xerath.SpawnXerath();
                    Xerath.RespawnTimer = 30.0f * 1000;
                }
            }

            foreach (var crystal in CaptureCrystals)
            {
                if (crystal.IsDead)
                {
                    crystal.RespawnTimer -= diff;
                    if (crystal.RespawnTimer <= 0 || forceSpawn)
                    {
                        crystal.SpawnCrystal();
                        crystal.RespawnTimer = 20.0f * 1000f;
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
            CaptureCrystals.Add(new AscensionCrystal(new Vector2(5023.0f, 7765.0f)));
            CaptureCrystals.Add(new AscensionCrystal(new Vector2(8838.0f, 7760.0f)));
            CaptureCrystals.Add(new AscensionCrystal(new Vector2(6960.0f, 4080.0f)));

            LogInfo(CaptureCrystals.Count.ToString());
            var speedShrine1 = CreateJungleCamp(new Vector3(5022.9287f, 60.0f, 7778.2695f), 102, 0, "NoIcon", 0);
            SpeedShrines.Add(speedShrine1, CreateJungleMonster("OdinSpeedShrine", "OdinSpeedShrine", new Vector2(5022.9287f, 7778.2695f), new Vector3(-0.0f, 0.0f, 1.0f), speedShrine1, isTargetable: false, ignoresCollision: true));

            var speedShrine2 = CreateJungleCamp(new Vector3(8859.897f, 60.0f, 7788.1064f), 103, 0, "NoIcon", 0);
            SpeedShrines.Add(speedShrine2, CreateJungleMonster("OdinSpeedShrine", "OdinSpeedShrine", new Vector2(8859.897f, 7788.1064f), new Vector3(-0.0f, 0.0f, 1.0f), speedShrine2, isTargetable: false, ignoresCollision: true));

            var speedShrine3 = CreateJungleCamp(new Vector3(6962.6934f, 60.0f, 4089.48f), 104, 0, "NoIcon", 0);
            SpeedShrines.Add(speedShrine3, CreateJungleMonster("OdinSpeedShrine", "OdinSpeedShrine", new Vector2(6962.6934f, 4089.48f), new Vector3(-0.0f, 0.0f, 1.0f), speedShrine3, isTargetable: false, ignoresCollision: true));


            var healthPacket1 = CreateJungleCamp(new Vector3(4948.231f, 60.0f, 9329.905f), 100, 0, "HealthPack", 10.0f * 1000f);
            HealthPacks.Add(healthPacket1, CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(4948.231f, 9329.905f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket1));

            var healthPacket2 = CreateJungleCamp(new Vector3(8972.231f, 60.0f, 9329.905f), 101, 0, "HealthPack", 10.0f * 1000f);
            HealthPacks.Add(healthPacket2, CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(8972.231f, 9329.905f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket2));

            var healthPacket3 = CreateJungleCamp(new Vector3(6949.8193f, 60.0f, 2855.0513f), 112, 0, "HealthPack", 10.0f * 1000f);
            HealthPacks.Add(healthPacket3, CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(6949.8193f, 2855.0513f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket3));

            var healthPacket4 = CreateJungleCamp(new Vector3(4324.928f, 60.0f, 5500.919f), 110, 0, "HealthPack", 10.0f * 1000f);
            HealthPacks.Add(healthPacket4, CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(4324.928f, 5500.919f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket4));

            var healthPacket5 = CreateJungleCamp(new Vector3(9573.432f, 60.0f, 5530.13f), 111, 0, "HealthPack", 10.0f * 1000f);
            HealthPacks.Add(healthPacket5, CreateJungleMonster("OdinShieldRelic", "OdinShieldRelic", new Vector2(9573.432f, 5530.13f), new Vector3(-0.0f, 0.0f, 1.0f), healthPacket5));

            var xerath = CreateJungleCamp(new Vector3(6930.0f, 60.0f, 6443.0f), 0, 0, "Dragon", 30.0f * 1000);
            Xerath = new AscXerath(CreateJungleMonster("AscXerath", "AscXerath", new Vector2(6930.0f, 6443.0f), new Vector3(-0.0f, 0.0f, 1.0f), xerath, TeamId.TEAM_NEUTRAL));
        }
    }
}

public class AscXerath
{
    IMonster Xerath;
    public IMonsterCamp Camp;
    public float RespawnTimer { get; set; } = 30.0f * 1000;
    public AscXerath(IMonster xerath)
    {
        Xerath = xerath;
        Camp = xerath.Camp;
    }
    public void SpawnXerath()
    {
        var ascXerath = Camp.AddMonster(Xerath);
        NotifySpawnBroadcast(ascXerath);
        ascXerath.PauseAnimation(true);
        SetMinimapIcon(ascXerath, "Dragon", true);
    }
}

public class AscensionCrystal
{
    Vector2 Position;
    public float RespawnTimer { get; set; } = 20.0f * 1000;
    public bool IsDead { get; set; }
    public AscensionCrystal(Vector2 position)
    {
        Position = position;
        IsDead = true;
    }

    public void SpawnCrystal()
    {
        var crystal = CreateMinion("AscRelic", "AscRelic", Position, team: TeamId.TEAM_NEUTRAL, ignoreCollision: true, instantNotifyBroadcast: true);
        AddUnitPerceptionBubble(crystal, 800.0f, 25000.0f, TeamId.TEAM_BLUE, false, null, 38.08f);
        AddUnitPerceptionBubble(crystal, 800.0f, 25000.0f, TeamId.TEAM_PURPLE, false, null, 38.08f);
        SetMinimapIcon(crystal, "Relic", true);
        ApiEventManager.OnDeath.AddListener(crystal, crystal, OnDeath, true);
        IsDead = false;
    }

    public void OnDeath(IDeathData deathData)
    {
        IsDead = true;
    }
}