using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface ICharData
    {
        IGlobalData GlobalCharData { get; }
        float AcquisitionRange { get; }
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
        float DamagePerLevel { get; }
        float ExpGivenOnDeath { get; }
        float GameplayCollisionRadius { get; }
        float GlobalExpGivenOnDeath { get; }
        float GlobalGoldGivenOnDeath { get; }
        float GoldGivenOnDeath { get; }
        float HpPerLevel { get; }
        float HpRegenPerLevel { get; }
        bool IsMelee { get; } //Yes or no
        float LocalGoldGivenOnDeath { get; }
        int MoveSpeed { get; }
        float MpPerLevel { get; }
        float MpRegenPerLevel { get; }
        PrimaryAbilityResourceType ParType { get; }
        IPassiveData PassiveData { get; }
        float PathfindingCollisionRadius { get; }
        float PerceptionBubbleRadius { get; }
        float SpellBlock { get; }
        float SpellBlockPerLevel { get; }
        string[] SpellNames { get; }
        int[][] SpellsUpLevels { get; }
        int[] MaxLevels { get; }
        string[] ExtraSpells { get; }
        // TODO: Verify if we want this to be an array.
        void Load(string name);
    }
}