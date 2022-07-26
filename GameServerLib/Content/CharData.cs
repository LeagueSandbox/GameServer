using System;
using System.Collections.Generic;
using System.Linq;
using GameServerCore.Enums;

namespace LeagueSandbox.GameServer.Content
{
    public class PassiveData
    {
        public string PassiveAbilityName { get; set; } = "";
        public int[] PassiveLevels { get; set; } = new int[6];
        public string PassiveLuaName { get; set; } = "";
        public string PassiveNameStr { get; set; } = "";

        //TODO: Extend into handling several passives, when we decide on a format for that case.
    }

    public class CharData
    {
        public float AcquisitionRange { get; private set; } = 475;
        public bool AllyCanUse { get; private set; } = false;
        public bool AlwaysVisible { get; private set; } = false;
        public bool AlwaysUpdatePAR { get; private set; } = false;
        public float Armor { get; private set; } = 1.0f;
        public float ArmorPerLevel { get; private set; } = 1.0f;
        public float AttackCastTime { get; private set; } = 0.0f;
        public float AttackDelayCastOffsetPercent { get; private set; } = 0.0f;
        public float AttackDelayCastOffsetPercentAttackSpeedRatio { get; private set; } = 0.0f;
        public float AttackDelayOffsetPercent { get; private set; } = 0.0f;
        public float AttackRange { get; private set; } = 100.0f;
        public float AttackSpeedPerLevel { get; private set; }
        public float AttackTotalTime { get; private set; } = 0.0f;
        public float BaseAttackProbability { get; private set; } = 0.5f;
        public float BaseDamage { get; private set; } = 10.0f;
        public float BaseHp { get; private set; } = 100.0f;
        public float BaseMp { get; private set; } = 100.0f;
        public float BaseStaticHpRegen { get; private set; } = 0.30000001f;
        public float BaseStaticMpRegen { get; private set; } = 0.30000001f;
        public float CooldownSpellSlot { get; private set; } = 0.0f;
        public float CritDamageBonus { get; private set; } = 2.0f;
        public float CritAttackDelayCastOffsetPercent { get; private set; } = 0.0f;
        public float CritAttackDelayCastOffsetPercentAttackSpeedRatio { get; private set; } = 0.0f;
        public float CritAttackDelayOffsetPercent { get; private set; } = 0.0f;
        public float CritAttackProbability { get; private set; } = 0.0f;
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

        public List<BasicAttackInfo> BasicAttacks { get; private set; } = Enumerable.Repeat(new BasicAttackInfo(), 18).ToList();

        // TODO: Verify if we want this to be an array.
        public PassiveData PassiveData { get; private set; } = new PassiveData();

        public CharData Load(ContentFile file)
        {
            string name = file.Name;

            AcquisitionRange = file.GetFloat("Data", "AcquisitionRange", AcquisitionRange);
            Armor = file.GetFloat("Data", "Armor", Armor);
            ArmorPerLevel = file.GetFloat("Data", "ArmorPerLevel", ArmorPerLevel);

            AttackCastTime = file.GetFloat("Data", "AttackCastTime", AttackCastTime);
            AttackDelayCastOffsetPercent = file.GetFloat("Data", "AttackDelayCastOffsetPercent", AttackDelayCastOffsetPercent);
            AttackDelayCastOffsetPercentAttackSpeedRatio = file.GetFloat("Data", "AttackDelayCastOffsetPercentAttackSpeedRatio", AttackDelayCastOffsetPercentAttackSpeedRatio);
            AttackDelayOffsetPercent = file.GetFloat("Data", "AttackDelayOffsetPercent", AttackDelayOffsetPercent);
            AttackRange = file.GetFloat("Data", "AttackRange", AttackRange);
            AttackSpeedPerLevel = file.GetFloat("Data", "AttackSpeedPerLevel", AttackSpeedPerLevel);
            AttackTotalTime = file.GetFloat("Data", "AttackTotalTime", AttackTotalTime);

            BaseAttackProbability = file.GetFloat("Data", "BaseAttack_Probability", BaseAttackProbability);
            BaseDamage = file.GetFloat("Data", "BaseDamage", BaseDamage);
            BaseHp = file.GetFloat("Data", "BaseHP", BaseHp);
            BaseMp = file.GetFloat("Data", "BaseMP", BaseMp);
            BaseStaticHpRegen = file.GetFloat("Data", "BaseStaticHPRegen", BaseStaticHpRegen);
            BaseStaticMpRegen = file.GetFloat("Data", "BaseStaticMPRegen", BaseStaticMpRegen);

            CritAttackDelayCastOffsetPercent = file.GetFloat("Data", "CritAttack_AttackDelayCastOffsetPercent", CritAttackDelayCastOffsetPercent);
            CritAttackDelayCastOffsetPercentAttackSpeedRatio = file.GetFloat("Data", "CritAttack_AttackDelayCastOffsetPercentAttackSpeedRatio", CritAttackDelayCastOffsetPercentAttackSpeedRatio);
            CritAttackDelayOffsetPercent = file.GetFloat("Data", "CritAttack_AttackDelayOffsetPercent", CritAttackDelayOffsetPercent);
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

            //Main AutoAttack
            BasicAttacks[0] = new BasicAttackInfo(AttackDelayOffsetPercent, AttackDelayCastOffsetPercent, AttackDelayCastOffsetPercentAttackSpeedRatio)
            {
                Name = name + "BasicAttack",
                AttackCastTime = AttackCastTime,
                AttackTotalTime = AttackTotalTime,
                Probability = BaseAttackProbability
            };
            BasicAttacks[0].GetAttackValues();

            int nameIndex = 2;
            //Secondary/Extra AutoAttacks
            for (var i = 1; i < 9; i++)
            {
                var attackName = file.GetString("Data", $"ExtraAttack{i}", "");

                //AncientGolem for example, doesn't have his ExtraAttacks explicitly defined in his file, but it has "ExtraAttack_Probability" which implies the existance of ExtraAttacks
                if (string.IsNullOrEmpty(attackName) && file.HasMentionOf("Data", $"ExtraAttack{i}"))
                {
                    attackName = $"{name}BasicAttack{nameIndex}";
                }

                if (BasicAttacks.Find(x => x.Name == attackName) != null)
                {
                    nameIndex++;
                    continue;
                }
                float offsetPercent = AttackDelayCastOffsetPercent = file.GetFloat("Data", $"ExtraAttack{i}_AttackDelayCastOffsetPercent", AttackDelayCastOffsetPercent);

                BasicAttacks[i] = new BasicAttackInfo(AttackDelayOffsetPercent, offsetPercent, AttackDelayCastOffsetPercentAttackSpeedRatio)
                {
                    Name = attackName,
                    AttackCastTime = file.GetFloat("Data", $"ExtraAttack{i}_AttackCastTime", AttackCastTime),
                    AttackTotalTime = file.GetFloat("Data", $"ExtraAttack{i}_AttackTotalTime", AttackTotalTime),
                    Probability = file.GetFloat("Data", $"ExtraAttack{i}_Probability", BaseAttackProbability)
                };
                BasicAttacks[i].GetAttackValues();
                nameIndex++;
            }

            //Main Crit AutoAttack
            BasicAttacks[9] = new BasicAttackInfo(CritAttackDelayOffsetPercent, CritAttackDelayCastOffsetPercent, CritAttackDelayCastOffsetPercentAttackSpeedRatio)
            {
                Name = file.GetString("Data", $"CritAttack", ""),
                AttackCastTime = AttackCastTime,
                AttackTotalTime = AttackTotalTime,
                Probability = CritAttackProbability
            };
            BasicAttacks[9].GetAttackValues();

            //Secondary Crit AutoAttacks
            for (var i = 1; i < 9; i++)
            {
                var index = i + 9;
                var attackName = file.GetString("Data", $"ExtraCritAttack{i}", "");
                float delayOffset = file.GetFloat("Data", $"{attackName}_AttackDelayOffsetPercent", AttackDelayOffsetPercent);
                float delayCastOffsetPercent = file.GetFloat("Data", $"{attackName}_AttackDelayCastOffsetPercent", CritAttackDelayCastOffsetPercent);

                BasicAttacks[index] = new BasicAttackInfo(delayOffset, delayCastOffsetPercent, CritAttackDelayCastOffsetPercentAttackSpeedRatio)
                {
                    Name = attackName,
                    AttackCastTime = AttackCastTime,
                    AttackTotalTime = AttackTotalTime,
                    Probability = CritAttackProbability
                };
                BasicAttacks[index].GetAttackValues();
            }

            return this;
        }
    }
}
