using GameServerCore.Enums;
using System.Collections.Generic;

namespace LeagueSandbox.GameServer.Content
{
    public static class GlobalData
    {
        private static Dictionary<string, float> _constants;
        public static DefaultStatValues DefaultStatValues { get; private set; } = new DefaultStatValues();
        public static GlobalCharacterDataConstants GlobalCharacterDataConstants { get; private set; } = new GlobalCharacterDataConstants();
        public static DamageRatios DamageRatios { get; private set; } = new DamageRatios();
        public static ObjAIBaseVariables ObjAIBaseVariables { get; private set; } = new ObjAIBaseVariables();
        public static AttackRangeVariables AttackRangeVariables { get; private set; } = new AttackRangeVariables();
        public static AIAttackTargetSelectionVariables AIAttackTargetSelectionVariables { get; private set; } = new AIAttackTargetSelectionVariables();
        public static SpellVampVariables SpellVampVariables { get; private set; } = new SpellVampVariables();
        public static AttackFlags AttackFlags { get; private set; } = new AttackFlags();
        public static MinionEXPMods MinionEXPMods { get; private set; } = new MinionEXPMods();
        public static ChampionVariables ChampionVariables { get; private set; } = new ChampionVariables();
        public static ServerCulling ServerCulling { get; private set; } = new ServerCulling();
        public static BarrackVariables BarrackVariables { get; private set; } = new BarrackVariables();
        public static ObjAIBuildingVariables ObjAIBuildingVariables { get; private set; } = new ObjAIBuildingVariables();
        public static SpawnPointVariables SpawnPointVariables { get; private set; } = new SpawnPointVariables();
        public static NexusVariables NexusVariables { get; private set; } = new NexusVariables();
        public static CallForHelpVariables CallForHelpVariables { get; private set; } = new CallForHelpVariables();

        public static void Init(Dictionary<string, float> constants)
        {
            _constants = constants;

            DefaultStatValues.LocalGoldMulti = GetFloat("defaultstats_LocalGoldMulti", DefaultStatValues.LocalGoldMulti);

            GlobalCharacterDataConstants.AttackDelay = GetFloat("gcd_AttackDelay", GlobalCharacterDataConstants.AttackDelay);
            GlobalCharacterDataConstants.AttackDelayCastPercent = GetFloat("gcd_AttackDelayCastPercent", GlobalCharacterDataConstants.AttackDelayCastPercent);
            GlobalCharacterDataConstants.AttackMinDelay = GetFloat("gcd_AttackMinDelay", GlobalCharacterDataConstants.AttackMinDelay);
            GlobalCharacterDataConstants.PercentAttackSpeedModMinimum = GetFloat("gcd_PercentAttackSpeedModMinimum", GlobalCharacterDataConstants.PercentAttackSpeedModMinimum);
            GlobalCharacterDataConstants.AttackMaxDelay = GetFloat("gcd_AttackMaxDelay", GlobalCharacterDataConstants.AttackMaxDelay);
            GlobalCharacterDataConstants.CooldownMinimum = GetFloat("gcd_CooldownMinimum", GlobalCharacterDataConstants.CooldownMinimum);
            GlobalCharacterDataConstants.PercentRespawnTimeModMinimum = GetFloat("gcd_PercentRespawnTimeModMinimum", GlobalCharacterDataConstants.PercentRespawnTimeModMinimum);
            GlobalCharacterDataConstants.PercentGoldLostOnDeathModMinimum = GetFloat("gcd_PercentGoldLostOnDeathModMinimum", GlobalCharacterDataConstants.PercentGoldLostOnDeathModMinimum);
            GlobalCharacterDataConstants.PercentEXPBonusMinimum = GetFloat("gcd_PercentEXPBonusMinimum", GlobalCharacterDataConstants.PercentEXPBonusMinimum);
            GlobalCharacterDataConstants.PercentEXPBonusMaximum = GetFloat("gcd_PercentEXPBonusMaximum", GlobalCharacterDataConstants.PercentEXPBonusMaximum);
            GlobalCharacterDataConstants.PercentCooldownModMinimum = GetFloat("gcd_PercentCooldownModMinimum", GlobalCharacterDataConstants.PercentCooldownModMinimum);

            DamageRatios.HeroToHero = GetFloat("dr_HeroToHero", DamageRatios.HeroToHero);
            DamageRatios.BuildingToHero = GetFloat("dr_BuildingToHero", DamageRatios.BuildingToHero);
            DamageRatios.UnitToHero = GetFloat("dr_UnitToHero", DamageRatios.UnitToHero);
            DamageRatios.HeroToUnit = GetFloat("dr_HeroToUnit", DamageRatios.HeroToUnit);
            DamageRatios.BuildingToUnit = GetFloat("dr_BuildingToUnit", DamageRatios.BuildingToUnit);
            DamageRatios.UnitToUnit = GetFloat("dr_UnitToUnit", DamageRatios.UnitToUnit);
            DamageRatios.HeroToBuilding = GetFloat("dr_HeroToBuilding", DamageRatios.HeroToBuilding);
            DamageRatios.BuildingToBuilding = GetFloat("dr_BuildingToBuilding", DamageRatios.BuildingToBuilding);
            DamageRatios.UnitToBuilding = GetFloat("dr_UnitToBuilding", DamageRatios.UnitToBuilding);

            ObjAIBaseVariables.AIToggle = GetBool("ai_AIToggle", ObjAIBaseVariables.AIToggle);
            ObjAIBaseVariables.PathIgnoresBuildings = GetBool("ai_PathIgnoresBuildings", ObjAIBaseVariables.PathIgnoresBuildings);
            ObjAIBaseVariables.ExpRadius2 = GetFloat("ai_ExpRadius2", ObjAIBaseVariables.ExpRadius2);
            ObjAIBaseVariables.GoldRadius2 = GetFloat("ai_GoldRadius2", ObjAIBaseVariables.GoldRadius2);
            ObjAIBaseVariables.StartingGold = GetFloat("ai_StartingGold", ObjAIBaseVariables.StartingGold);
            ObjAIBaseVariables.DefaultPetReturnRadius = GetFloat("ai_DefaultPetReturnRadius", ObjAIBaseVariables.DefaultPetReturnRadius);
            ObjAIBaseVariables.AmbientGoldDelay = GetFloat("ai_AmbientGoldDelay", ObjAIBaseVariables.AmbientGoldDelay) * 1000.0f;
            ObjAIBaseVariables.AmbientGoldDelayFirstBlood = GetFloat("ai_AmbientGoldDelayFirstBlood", ObjAIBaseVariables.AmbientGoldDelayFirstBlood);

            AttackRangeVariables.ClosingAttackRangeModifier = GetFloat("ar_ClosingAttackRangeModifier", AttackRangeVariables.ClosingAttackRangeModifier);
            AttackRangeVariables.StopAttackRangeModifier = GetFloat("ar_StopAttackRangeModifier", AttackRangeVariables.StopAttackRangeModifier);
            AttackRangeVariables.AICharmedAcquisitionRange = GetFloat("ar_AICharmedAcquisitionRange", AttackRangeVariables.AICharmedAcquisitionRange);

            AIAttackTargetSelectionVariables.TargetDistanceFactorPerNeightbor = GetFloat("ai_TargetDistanceFactorPerNeightbor", AIAttackTargetSelectionVariables.TargetDistanceFactorPerNeightbor);
            AIAttackTargetSelectionVariables.TargetDistanceFactorPerAttacker = GetFloat("ai_TargetDistanceFactorPerAttacker", AIAttackTargetSelectionVariables.TargetDistanceFactorPerAttacker);
            AIAttackTargetSelectionVariables.TargetRangeFactor = GetFloat("ai_TargetRangeFactor", AIAttackTargetSelectionVariables.TargetRangeFactor);
            AIAttackTargetSelectionVariables.TargetPathFactor = GetFloat("ai_TargetPathFactor", AIAttackTargetSelectionVariables.TargetPathFactor);
            AIAttackTargetSelectionVariables.MinionTargetingHeroBoost = GetFloat("ai_MinionTargetingHeroBoost", AIAttackTargetSelectionVariables.MinionTargetingHeroBoost);
            AIAttackTargetSelectionVariables.TargetMaxNumAttackers = GetInt("ai_TargetMaxNumAttackers", AIAttackTargetSelectionVariables.TargetMaxNumAttackers);

            SpellVampVariables.SpellVampRatios[DamageSource.DAMAGE_SOURCE_SPELL] = GetFloat("sv_SpellRatio", SpellVampVariables.SpellVampRatios[DamageSource.DAMAGE_SOURCE_SPELL]);
            SpellVampVariables.SpellVampRatios[DamageSource.DAMAGE_SOURCE_SPELLAOE] = GetFloat("sv_SpellAoERatio", SpellVampVariables.SpellVampRatios[DamageSource.DAMAGE_SOURCE_SPELLAOE]);
            SpellVampVariables.SpellVampRatios[DamageSource.DAMAGE_SOURCE_SPELLPERSIST] = GetFloat("sv_SpellPersistRatio", SpellVampVariables.SpellVampRatios[DamageSource.DAMAGE_SOURCE_SPELLPERSIST]);
            SpellVampVariables.SpellVampRatios[DamageSource.DAMAGE_SOURCE_PERIODIC] = GetFloat("sv_PeriodicRatio", SpellVampVariables.SpellVampRatios[DamageSource.DAMAGE_SOURCE_PERIODIC]);
            SpellVampVariables.SpellVampRatios[DamageSource.DAMAGE_SOURCE_PROC] = GetFloat("sv_ProcRatio", SpellVampVariables.SpellVampRatios[DamageSource.DAMAGE_SOURCE_PROC]);
            SpellVampVariables.SpellVampRatios[DamageSource.DAMAGE_SOURCE_REACTIVE] = GetFloat("sv_ReactiveRatio", SpellVampVariables.SpellVampRatios[DamageSource.DAMAGE_SOURCE_REACTIVE]);
            SpellVampVariables.SpellVampRatios[DamageSource.DAMAGE_SOURCE_ONDEATH] = GetFloat("sv_OnDeathRatio", SpellVampVariables.SpellVampRatios[DamageSource.DAMAGE_SOURCE_ONDEATH]);
            SpellVampVariables.SpellVampRatios[DamageSource.DAMAGE_SOURCE_PET] = GetFloat("sv_PetRatio", SpellVampVariables.SpellVampRatios[DamageSource.DAMAGE_SOURCE_PET]);

            AttackFlags.RevealAttackerRange = GetFloat("ca_RevealAttackerRange", AttackFlags.RevealAttackerRange);
            AttackFlags.RevealAttackerTimeOut = GetFloat("ca_RevealAttackerTimeOut", AttackFlags.RevealAttackerTimeOut);
            AttackFlags.MinCastRotationSpeed = GetFloat("ca_MinCastRotationSpeed", AttackFlags.MinCastRotationSpeed);
            AttackFlags.TargetingReticleHeight = GetFloat("hud_targeting_reticle_height", AttackFlags.TargetingReticleHeight);

            MinionEXPMods.BonusExpPercentPerNeutralMinionLevel = GetFloat("aiExp_bonusExpPercentPerNeutralMinionLevel", MinionEXPMods.BonusExpPercentPerNeutralMinionLevel);
            MinionEXPMods.BonusExpPercentPerLaneMinionLevel = GetFloat("aiExp_bonusExpPercentPerLaneMinionLevel", MinionEXPMods.BonusExpPercentPerLaneMinionLevel);
            MinionEXPMods.BonusExpLevelDeltaCap = GetInt("aiExp_bonusExpLevelDeltaCap", MinionEXPMods.BonusExpLevelDeltaCap);
            MinionEXPMods.BonusExpLaneLevelStart = GetInt("aiExp_bonusExpLaneLevelStart", MinionEXPMods.BonusExpLaneLevelStart);
            MinionEXPMods.BonusExpLaneLevelDeltaMin = GetInt("aiExp_bonusExpLaneLevelDeltaMin", MinionEXPMods.BonusExpLaneLevelDeltaMin);

            ChampionVariables.AmbientGoldInterval = GetFloat("ai_AmbientGoldInterval", ChampionVariables.AmbientGoldInterval);
            //Division by 5 is due to the stat itself being "gold per 5 seconds"
            ChampionVariables.AmbientGoldAmount = GetFloat("ai_AmbientGoldAmount", ChampionVariables.AmbientGoldAmount) / (10 / ChampionVariables.AmbientGoldInterval) / 5;
            ChampionVariables.AmbientGoldInterval *= 100.0f;
            ChampionVariables.DisableAmbientGoldWhileDead = GetBool("ai_DisableAmbientGoldWhileDead", ChampionVariables.DisableAmbientGoldWhileDead);
            ChampionVariables.AmbientXPDelay = GetFloat("ai_AmbientXPDelay", ChampionVariables.AmbientXPDelay) * 1000.0f;
            ChampionVariables.AmbientXPInterval = GetFloat("ai_AmbientXPInterval", ChampionVariables.AmbientXPInterval) * 1000.0f;
            ChampionVariables.AmbientXPAmount = GetFloat("ai_AmbientXPAmount", ChampionVariables.AmbientXPAmount);
            ChampionVariables.AmbientXPAmountTutorial = GetFloat("ai_AmbientXPAmountTutorial", ChampionVariables.AmbientXPAmountTutorial);
            ChampionVariables.DisableAmbientXPWhileDead = GetBool("ai_DisableAmbientXPWhileDead", ChampionVariables.DisableAmbientXPWhileDead);
            ChampionVariables.GoldLostPerLevel = GetFloat("ai_GoldLostPerLevel", ChampionVariables.GoldLostPerLevel);
            ChampionVariables.GoldHandicapCoefficient = GetFloat("ai_GoldHandicapCoefficient", ChampionVariables.GoldHandicapCoefficient);
            ChampionVariables.TimeDeadPerLevel = GetFloat("ai_TimeDeadPerLevel", ChampionVariables.TimeDeadPerLevel);
            ChampionVariables.TimeForMultiKill = GetFloat("events_TimeForMultiKill", ChampionVariables.TimeForMultiKill);
            ChampionVariables.TimeForLastMultiKill = GetFloat("events_TimeForLastMultiKill", ChampionVariables.TimeForLastMultiKill);
            ChampionVariables.TimerForAssist = GetFloat("events_TimerForAssist", ChampionVariables.TimerForAssist);
            ChampionVariables.MinionDenialPercentage = GetFloat("ai_MinionDenialPercentage", ChampionVariables.MinionDenialPercentage);

            ServerCulling.ClosenessLineOfSightThresholdTurret = GetFloat("ser_ClosenessLineOfSightThresholdTurret", ServerCulling.ClosenessLineOfSightThresholdTurret);

            BarrackVariables.BSpawnEnabled = GetBool("bar_bSpawnEnabled", BarrackVariables.BSpawnEnabled);
            BarrackVariables.Armor = GetInt("bar_Armor", BarrackVariables.Armor);
            BarrackVariables.MaxHP = GetInt("bar_MaxHP", BarrackVariables.MaxHP);
            BarrackVariables.MaxHPTutorial = GetInt("bar_MaxHPTutorial", BarrackVariables.MaxHPTutorial);

            ObjAIBuildingVariables.TimerForBuildingKillCredit = GetFloat("events_TimerForBuildingKillCredit", ObjAIBuildingVariables.TimerForBuildingKillCredit);
            ObjAIBuildingVariables.TimerBeforeSendingDamageEvent = GetFloat("events_TimerBeforeSendingDamageEvent", ObjAIBuildingVariables.TimerBeforeSendingDamageEvent);
            ObjAIBuildingVariables.MinimumHealthForDamageEvent = GetFloat("events_MinimumHealthForDamageEvent", ObjAIBuildingVariables.MinimumHealthForDamageEvent);
            ObjAIBuildingVariables.MinimumNumberOfMinionsForDamageEvent = GetFloat("events_MinimumNumberOfMinionsForDamageEvent", ObjAIBuildingVariables.MinimumNumberOfMinionsForDamageEvent);
            ObjAIBuildingVariables.DamageEventRadius = GetFloat("events_DamageEventRadius", ObjAIBuildingVariables.DamageEventRadius);
            ObjAIBuildingVariables.ConstantAttackTimeForDamageEvent = GetFloat("events_ConstantAttackTimeForDamageEvent", ObjAIBuildingVariables.ConstantAttackTimeForDamageEvent);
            ObjAIBuildingVariables.NoDamageCancelTime = GetFloat("events_NoDamageCancelTime", ObjAIBuildingVariables.NoDamageCancelTime);

            SpawnPointVariables.RegenRadius = GetFloat("sp_RegenRadius", SpawnPointVariables.RegenRadius);
            SpawnPointVariables.HealthRegenPercent = GetFloat("sp_HealthRegenPercent", SpawnPointVariables.HealthRegenPercent);
            SpawnPointVariables.HealthRegenPercentARAM = GetFloat("sp_HealthRegenPercentARAM", SpawnPointVariables.HealthRegenPercentARAM);
            SpawnPointVariables.ManaRegenPercent = GetFloat("sp_ManaRegenPercent", SpawnPointVariables.ManaRegenPercent);
            SpawnPointVariables.RegenTickInterval = GetFloat("sp_RegenTickInterval", SpawnPointVariables.RegenTickInterval);

            NexusVariables.EoGPanTime = GetFloat("hq_EoGPanTime", NexusVariables.EoGPanTime);
            NexusVariables.EoGNexusExplosionTime = GetFloat("hq_EoGNexusExplosionTime", NexusVariables.EoGNexusExplosionTime);
            NexusVariables.EoGUseNexusDeathAnimation = GetBool("hq_EoGUseNexusDeathAnimation", NexusVariables.EoGUseNexusDeathAnimation);
            NexusVariables.EoGNexusChangeSkinTime = GetFloat("hq_EoGNexusChangeSkinTime", NexusVariables.EoGNexusChangeSkinTime);

            CallForHelpVariables.Delay = GetFloat("cfh_Delay", CallForHelpVariables.Delay);
            CallForHelpVariables.Stick = GetFloat("cfh_Stick", CallForHelpVariables.Stick);
            CallForHelpVariables.Radius = GetFloat("cfh_Radius", CallForHelpVariables.Radius);
            CallForHelpVariables.Duration = GetFloat("cfh_Duration", CallForHelpVariables.Duration);
            CallForHelpVariables.MeleeRadius = GetFloat("cfh_MeleeRadius", CallForHelpVariables.MeleeRadius);
            CallForHelpVariables.RangedRadius = GetFloat("cfh_RangedRadius", CallForHelpVariables.RangedRadius);
            CallForHelpVariables.TurretRadius = GetFloat("cfh_TurretRadius", CallForHelpVariables.TurretRadius);
        }

        static int GetInt(string name, int defaultValue)
        {
            return (int)GetFloat(name, defaultValue);
        }

        static float GetFloat(string name, float defaultValue)
        {
            if (_constants.TryGetValue(name, out var value))
            {
                return value;
            }
            return defaultValue;
        }

        static bool GetBool(string name, bool defaultValue)
        {
            if (_constants.TryGetValue(name, out var value))
            {
                return value != 0;
            }
            return defaultValue;
        }
    }
}
