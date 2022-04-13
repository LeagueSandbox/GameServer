using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using GameServerCore.Domain;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Logging;
using log4net;
using Newtonsoft.Json;

namespace LeagueSandbox.GameServer.Content
{
    public class PassiveData : IPassiveData
    {
        public string PassiveAbilityName { get; set; } = "";
        public int[] PassiveLevels { get; set; } = new int[6];
        public string PassiveLuaName { get; set; } = "";
        public string PassiveNameStr { get; set; } = "";

        //TODO: Extend into handling several passives, when we decide on a format for that case.
    }

    public class CharData : ICharData
    {
        private readonly ContentManager _contentManager;
        private readonly ILog _logger;

        public CharData(ContentManager contentManager)
        {
            _contentManager = contentManager;
            _logger = LoggerProvider.GetLogger();
        }

        public IGlobalData GlobalCharData { get; private set; } = new GlobalData();

        public float AcquisitionRange { get; private set; } = 475;
        public bool AllyCanUse { get; private set; } = false;
        public bool AlwaysVisible { get; private set; } = false;
        public bool AlwaysUpdatePAR { get; private set; } = false;
        public float Armor { get; private set; } = 1.0f;
        public float ArmorPerLevel { get; private set; } = 1.0f;
        public float[] AttackDelayCastOffsetPercent { get; private set; } = new float[18];
        public float[] AttackDelayCastOffsetPercentAttackSpeedRatio { get; private set; } = new float[18];
        public float[] AttackDelayOffsetPercent { get; private set; } = new float[18];
        public float AttackRange { get; private set; } = 100.0f;
        public float AttackSpeedPerLevel { get; private set; }
        public float BaseDamage { get; private set; } = 10.0f;
        public float BaseHp { get; private set; } = 100.0f;
        public float BaseMp { get; private set; } = 100.0f;
        public float BaseStaticHpRegen { get; private set; } = 0.30000001f;
        public float BaseStaticMpRegen { get; private set; } = 0.30000001f;
        public float CooldownSpellSlot { get; private set; } = 0.0f;
        public float CritDamageBonus { get; private set; } = 2.0f;
        public float DamagePerLevel { get; private set; } = 10.0f;
        public bool DisableContinuousTargetFacing { get; private set; } = false;
        public bool EnemyCanUse { get; private set; } = false;
        public float ExpGivenOnDeath { get; private set; } = 0.0f;
        public float GameplayCollisionRadius { get; private set; } = 65.0f;
        public float GlobalExpGivenOnDeath { get; private set; } = 0.0f;
        public float GlobalGoldGivenOnDeath { get; private set; } = 0.0f;
        public float GoldGivenOnDeath { get; private set; } = 0.0f;
        public string HeroUseSpell { get; private set; } = string.Empty;
        public float HpPerLevel { get; private set; } = 10.0f;
        public float HpRegenPerLevel { get; private set; }
        public bool IsMelee { get; private set; } //Yes or no
        public bool Immobile { get; private set; } = false;
        public bool IsTower { get; private set; } = false;
        public bool IsUseable { get; private set; } = false;
        public float LocalGoldGivenOnDeath { get; private set; } = 0.0f;
        public bool MinionUseable { get; private set; } = false;
        public string MinionUseSpell { get; private set; } = string.Empty;
        public int MoveSpeed { get; private set; } = 100;
        public float MpPerLevel { get; private set; } = 10.0f;
        public float MpRegenPerLevel { get; private set; }
        public PrimaryAbilityResourceType ParType { get; private set; } = PrimaryAbilityResourceType.MANA;
        public float PathfindingCollisionRadius { get; private set; } = -1.0f;
        public float PerceptionBubbleRadius { get; private set; } = 0.0f;
        public bool ShouldFaceTarget { get; private set; } = true;
        public float SpellBlock { get; private set; }
        public float SpellBlockPerLevel { get; private set; }
        public UnitTag UnitTags { get; private set; }

        public string[] SpellNames { get; private set; } = new string[4];
        public string[] ExtraSpells { get; private set; } = new string[16];
        public int[] MaxLevels { get; private set; } = { 5, 5, 5, 3 };

        public int[][] SpellsUpLevels { get; private set; } =
        {
            new[] {1, 3, 5, 7, 9, 99},
            new[] {1, 3, 5, 7, 9, 99},
            new[] {1, 3, 5, 7, 9, 99},
            new[] {6, 11, 16, 99, 99, 99}
        };

        public string[] AttackNames { get; private set; } = new string[18];
        public float[] AttackProbabilities { get; private set; } = new float[18];

        // TODO: Verify if we want this to be an array.
        public IPassiveData PassiveData { get; private set; } = new PassiveData();

        public void Load(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return;
            }

            var file = new ContentFile();
            List<IPackage> packages;
            try
            {
                file = (ContentFile)_contentManager.GetContentFileFromJson("Stats", name);
                packages = new List<IPackage>(_contentManager.GetAllLoadedPackages());
            }
            catch (ContentNotFoundException exception)
            {
                _logger.Warn(exception.Message);
                return;
            }

            AcquisitionRange = file.GetFloat("Data", "AcquisitionRange", AcquisitionRange);
            Armor = file.GetFloat("Data", "Armor", Armor);
            ArmorPerLevel = file.GetFloat("Data", "ArmorPerLevel", ArmorPerLevel);
            AttackRange = file.GetFloat("Data", "AttackRange", AttackRange);
            AttackSpeedPerLevel = file.GetFloat("Data", "AttackSpeedPerLevel", AttackSpeedPerLevel);
            BaseDamage = file.GetFloat("Data", "BaseDamage", BaseDamage);
            BaseHp = file.GetFloat("Data", "BaseHP", BaseHp);
            BaseMp = file.GetFloat("Data", "BaseMP", BaseMp);
            BaseStaticHpRegen = file.GetFloat("Data", "BaseStaticHPRegen", BaseStaticHpRegen);
            BaseStaticMpRegen = file.GetFloat("Data", "BaseStaticMPRegen", BaseStaticMpRegen);
            CritDamageBonus = file.GetFloat("Data", "CritDamageBonus", CritDamageBonus);
            DamagePerLevel = file.GetFloat("Data", "DamagePerLevel", DamagePerLevel);
            DisableContinuousTargetFacing = file.GetBool("Data", "DisableContinuousTargetFacing");
            ExpGivenOnDeath = file.GetFloat("Data", "ExpGivenOnDeath", ExpGivenOnDeath);
            GameplayCollisionRadius = file.GetFloat("Data", "GameplayCollisionRadius", GameplayCollisionRadius);
            GlobalExpGivenOnDeath = file.GetFloat("Data", "GlobalExpGivenOnDeath", GlobalExpGivenOnDeath);
            GlobalGoldGivenOnDeath = file.GetFloat("Data", "GlobalGoldGivenOnDeath", GlobalGoldGivenOnDeath);
            GoldGivenOnDeath = file.GetFloat("Data", "GoldGivenOnDeath", GoldGivenOnDeath);
            HpRegenPerLevel = file.GetFloat("Data", "HPRegenPerLevel", HpRegenPerLevel);
            HpPerLevel = file.GetFloat("Data", "HPPerLevel", HpPerLevel);
            Immobile = file.GetBool("Data", "Imobile", Immobile);
            IsMelee = file.GetString("Data", "IsMelee", IsMelee ? "true" : "false").Equals("true");
            LocalGoldGivenOnDeath = file.GetFloat("Data", "LocalGoldGivenOnDeath", LocalGoldGivenOnDeath);
            MoveSpeed = file.GetInt("Data", "MoveSpeed", MoveSpeed);
            MpRegenPerLevel = file.GetFloat("Data", "MPRegenPerLevel", MpRegenPerLevel);
            MpPerLevel = file.GetFloat("Data", "MPPerLevel", MpPerLevel);
            PathfindingCollisionRadius = file.GetFloat("Data", "PathfindingCollisionRadius", PathfindingCollisionRadius);
            PerceptionBubbleRadius = file.GetFloat("Data", "PerceptionBubbleRadius", PerceptionBubbleRadius);
            ShouldFaceTarget = file.GetBool("Data", "ShouldFaceTarget", ShouldFaceTarget);
            SpellBlock = file.GetFloat("Data", "SpellBlock", SpellBlock);
            SpellBlockPerLevel = file.GetFloat("Data", "SpellBlockPerLevel", SpellBlockPerLevel);

            EnemyCanUse = file.GetBool("Useable", "EnemyCanUse", EnemyCanUse);
            AllyCanUse = file.GetBool("Useable", "AllyCanUse", AllyCanUse);
            HeroUseSpell = file.GetString("Useable", "HeroUseSpell", HeroUseSpell);
            CooldownSpellSlot = file.GetFloat("Useable", "CooldownSpellSlot", CooldownSpellSlot);
            IsUseable = file.GetBool("Useable", "IsUseable", IsUseable);
            MinionUseable = file.GetBool("Useable", "MinionUseable", MinionUseable);
            MinionUseSpell = file.GetString("Useable", "MinionUseSpell", MinionUseSpell);

            AlwaysVisible = file.GetBool("Minion", "AlwaysVisible", AlwaysVisible);
            IsTower = file.GetBool("Minion", "IsTower", IsTower);
            AlwaysUpdatePAR = file.GetBool("Minion", "AlwaysUpdatePAR", AlwaysUpdatePAR);

            foreach (var tag in file.GetString("Data", "UnitTags").Split(" | "))
            {
                Enum.TryParse(tag, out UnitTag unitTag);
                UnitTags |= unitTag;
            }

            Enum.TryParse<PrimaryAbilityResourceType>(file.GetString("Data", "PARType", ParType.ToString()),
                out var tempPar);
            ParType = tempPar;

            for (var i = 0; i < 4; i++)
            {
                SpellNames[i] = file.GetString("Data", $"Spell{i + 1}", "");
            }

            for (var i = 0; i < 16; i++)
            {
                ExtraSpells[i] = file.GetString("Data", $"ExtraSpell{i + 1}", "");
            }

            for (var i = 0; i < 4; i++)
            {
                SpellsUpLevels[i] = file.GetIntArray("Data", $"SpellsUpLevels{i + 1}", SpellsUpLevels[i]);
            }

            PassiveData.PassiveLuaName = file.GetString("Data", "Passive1LuaName", "");

            MaxLevels = file.GetIntArray("Data", "MaxLevels", MaxLevels);

            for (var i = 0; i < 18; i++)
            {
                if (i < 9)
                {
                    if (i == 0)
                    {
                        AttackNames[i] = name + "BasicAttack";
                        AttackProbabilities[i] = file.GetFloat("Data", "BaseAttack_Probability", 1.0f);
                        float initAttackCastTime = file.GetFloat("Data", "AttackCastTime", 0.0f);
                        float initAttackDelayOffsetPercent = file.GetFloat("Data", "AttackDelayOffsetPercent", 0.0f);
                        float initAttackDelayCastOffsetPercent = file.GetFloat("Data", "AttackDelayCastOffsetPercent", 0.0f);
                        float initAttackDelayCastOffsetPercentAttackSpeedRatio = file.GetFloat("Data", "AttackDelayCastOffsetPercentAttackSpeedRatio", 1.0f);
                        float initAttackTotalTime = file.GetFloat("Data", "AttackTotalTime", 0.0f);
                        float attackCastTime = Math.Min(initAttackTotalTime, initAttackCastTime);
                        if (initAttackTotalTime > 0.0f && attackCastTime > 0.0f)
                        {
                            AttackDelayOffsetPercent[i] = (initAttackTotalTime / GlobalCharData.AttackDelay) - 1.0f;
                            AttackDelayCastOffsetPercent[i] = (attackCastTime / initAttackTotalTime) - GlobalCharData.AttackDelayCastPercent;
                            AttackDelayCastOffsetPercentAttackSpeedRatio[i] = 1.0f;
                        }
                        else
                        {
                            AttackDelayOffsetPercent[i] = initAttackDelayOffsetPercent;
                            AttackDelayCastOffsetPercent[i] = Math.Max(initAttackDelayCastOffsetPercent, -GlobalCharData.AttackDelayCastPercent);
                            AttackDelayCastOffsetPercentAttackSpeedRatio[i] = initAttackDelayCastOffsetPercentAttackSpeedRatio;
                        }
                        continue;
                    }
                    else
                    {
                        AttackNames[i] = file.GetString("Data", $"ExtraAttack{i}", "");
                        AttackProbabilities[i] = file.GetFloat("Data", $"ExtraAttack{i}_Probability", 0.0f);
                        float extraAttackCastTime = file.GetFloat("Data", AttackNames[i] + "_AttackCastTime", 0.0f);
                        float extraAttackDelayOffsetPercent = file.GetFloat("Data", AttackNames[i] + "_AttackDelayOffsetPercent", AttackDelayOffsetPercent[0]);
                        float extraAttackDelayCastOffsetPercent = file.GetFloat("Data", AttackNames[i] + "_AttackDelayCastOffsetPercent", AttackDelayCastOffsetPercent[0]);
                        float extraAttackDelayCastOffsetPercentAttackSpeedRatio = file.GetFloat("Data", AttackNames[i] + "_AttackDelayCastOffsetPercentAttackSpeedRatio", 1.0f);
                        float extraAttackTotalTime = file.GetFloat("Data", AttackNames[i] + "_AttackTotalTime", 0.0f);
                        float attackCastTime = Math.Min(extraAttackTotalTime, extraAttackCastTime);
                        if (extraAttackTotalTime > 0.0f && attackCastTime > 0.0f)
                        {
                            AttackDelayOffsetPercent[i] = (extraAttackTotalTime / GlobalCharData.AttackDelay) - 1.0f;
                            AttackDelayCastOffsetPercent[i] = (attackCastTime / extraAttackTotalTime) - GlobalCharData.AttackDelayCastPercent;
                            AttackDelayCastOffsetPercentAttackSpeedRatio[i] = 1.0f;
                        }
                        else
                        {
                            AttackDelayOffsetPercent[i] = extraAttackDelayOffsetPercent;
                            AttackDelayCastOffsetPercent[i] = Math.Max(extraAttackDelayCastOffsetPercent, -GlobalCharData.AttackDelayCastPercent);
                            AttackDelayCastOffsetPercentAttackSpeedRatio[i] = extraAttackDelayCastOffsetPercentAttackSpeedRatio;
                        }
                    }
                }
                else if (i == 9)
                {
                    AttackNames[i] = name + "CritAttack";
                    AttackProbabilities[i] = file.GetFloat("Data", $"CritAttack_Probability", 1.0f);
                    float initAttackCastTime = file.GetFloat("Data", "CritAttack_AttackCastTime", 0.0f);
                    float initAttackDelayOffsetPercent = file.GetFloat("Data", "CritAttack_AttackDelayOffsetPercent", AttackDelayOffsetPercent[0]);
                    float initAttackDelayCastOffsetPercent = file.GetFloat("Data", "CritAttack_AttackDelayCastOffsetPercent", AttackDelayCastOffsetPercent[0]);
                    float initAttackDelayCastOffsetPercentAttackSpeedRatio = file.GetFloat("Data", "CritAttack_AttackDelayCastOffsetPercentAttackSpeedRatio", 1.0f);
                    float initAttackTotalTime = file.GetFloat("Data", "CritAttack_AttackTotalTime", 0.0f);
                    float attackCastTime = Math.Min(initAttackTotalTime, initAttackCastTime);
                    if (initAttackTotalTime > 0.0f && attackCastTime > 0.0f)
                    {
                        AttackDelayOffsetPercent[i] = (initAttackTotalTime / GlobalCharData.AttackDelay) - 1.0f;
                        AttackDelayCastOffsetPercent[i] = (attackCastTime / initAttackTotalTime) - GlobalCharData.AttackDelayCastPercent;
                        AttackDelayCastOffsetPercentAttackSpeedRatio[i] = 1.0f;
                    }
                    else
                    {
                        AttackDelayOffsetPercent[i] = initAttackDelayOffsetPercent;
                        AttackDelayCastOffsetPercent[i] = Math.Max(initAttackDelayCastOffsetPercent, -GlobalCharData.AttackDelayCastPercent);
                        AttackDelayCastOffsetPercentAttackSpeedRatio[i] = initAttackDelayCastOffsetPercentAttackSpeedRatio;
                    }
                    continue;
                }
                else if (AttackNames[9] != null)
                {
                    AttackNames[i] = file.GetString("Data", $"ExtraCritAttack{i}", "");
                    AttackProbabilities[i] = file.GetFloat("Data", $"ExtraCritAttack{i}_Probability", 0.0f);
                    float extraAttackCastTime = file.GetFloat("Data", AttackNames[i] + "_AttackCastTime", 0.0f);
                    float extraAttackDelayOffsetPercent = file.GetFloat("Data", AttackNames[i] + "_AttackDelayOffsetPercent", AttackDelayOffsetPercent[0]);
                    float extraAttackDelayCastOffsetPercent = file.GetFloat("Data", AttackNames[i] + "_AttackDelayCastOffsetPercent", AttackDelayCastOffsetPercent[0]);
                    float extraAttackDelayCastOffsetPercentAttackSpeedRatio = file.GetFloat("Data", AttackNames[i] + "_AttackDelayCastOffsetPercentAttackSpeedRatio", 1.0f);
                    float extraAttackTotalTime = file.GetFloat("Data", AttackNames[i] + "_AttackTotalTime", 0.0f);
                    float attackCastTime = Math.Min(extraAttackTotalTime, extraAttackCastTime);
                    if (extraAttackTotalTime > 0.0f && attackCastTime > 0.0f)
                    {
                        AttackDelayOffsetPercent[i] = (extraAttackTotalTime / GlobalCharData.AttackDelay) - 1.0f;
                        AttackDelayCastOffsetPercent[i] = (attackCastTime / extraAttackTotalTime) - GlobalCharData.AttackDelayCastPercent;
                        AttackDelayCastOffsetPercentAttackSpeedRatio[i] = 1.0f;
                    }
                    else
                    {
                        AttackDelayOffsetPercent[i] = extraAttackDelayOffsetPercent;
                        AttackDelayCastOffsetPercent[i] = Math.Max(extraAttackDelayCastOffsetPercent, -GlobalCharData.AttackDelayCastPercent);
                        AttackDelayCastOffsetPercentAttackSpeedRatio[i] = extraAttackDelayCastOffsetPercentAttackSpeedRatio;
                    }
                }
            }
        }
    }
}
