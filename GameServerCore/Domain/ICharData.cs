using GameServerCore.Enums;
using System.Collections.Generic;

namespace GameServerCore.Domain
{
    public interface ICharData
    {
        IGlobalData GlobalCharData { get; }
        float AcquisitionRange { get; }
        bool AllyCanUse { get; }
        bool AlwaysVisible { get; }
        bool AlwaysUpdatePAR { get; }
        float Armor { get; }
        float ArmorPerLevel { get; }
        float[] AttackDelayCastOffsetPercent { get; }
        float[] AttackDelayCastOffsetPercentAttackSpeedRatio { get; }
        float[] AttackDelayOffsetPercent { get; }
        string[] AttackNames { get; }
        float[] AttackProbabilities { get; }
        float AttackRange { get; }
        float AttackSpeedPerLevel { get; }
        float BaseDamage { get; }
        float BaseHp { get; }
        float BaseMp { get; }
        float BaseStaticHpRegen { get; }
        float BaseStaticMpRegen { get; }
        float CooldownSpellSlot { get; }
        float CritDamageBonus { get; }
        float DamagePerLevel { get; }
        bool EnemyCanUse { get; }
        float ExpGivenOnDeath { get; }
        float GameplayCollisionRadius { get; }
        float GlobalExpGivenOnDeath { get; }
        float GlobalGoldGivenOnDeath { get; }
        float GoldGivenOnDeath { get; }
        string HeroUseSpell { get; }
        float HpPerLevel { get; }
        float HpRegenPerLevel { get; }
        bool IsMelee { get; } //Yes or no
        bool Immobile { get; }
        bool IsUseable { get; }
        float LocalGoldGivenOnDeath { get; }
        bool MinionUseable { get; }
        int MoveSpeed { get; }
        float MpPerLevel { get; }
        float MpRegenPerLevel { get; }
        PrimaryAbilityResourceType ParType { get; }
        IPassiveData PassiveData { get; }
        float PathfindingCollisionRadius { get; }
        float PerceptionBubbleRadius { get; }
        bool ShouldFaceTarget { get; }
        float SpellBlock { get; }
        float SpellBlockPerLevel { get; }
        string[] SpellNames { get; }
        int[][] SpellsUpLevels { get; }
        UnitTag UnitTags { get; }
        int[] MaxLevels { get; }
        string[] ExtraSpells { get; }
        // TODO: Verify if we want this to be an array.
        void Load(string name);
    }
}