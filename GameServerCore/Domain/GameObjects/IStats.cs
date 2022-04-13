using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface IStats: IUpdate
    {
        ulong SpellsEnabled { get; }
        ulong SummonerSpellsEnabled { get; }
        ActionState ActionState { get; }
        PrimaryAbilityResourceType ParType { get; }
        bool IsMagicImmune { get; }
        bool IsInvulnerable { get; }
        bool IsPhysicalImmune { get; }
        bool IsLifestealImmune { get; }
        bool IsTargetable { get; set; }
        SpellDataFlags IsTargetableToTeam { get; set; }
        float AttackSpeedFlat { get; set; }
        float HealthPerLevel { get; }
        float ManaPerLevel { get; }
        float ArmorPerLevel { get; }
        float MagicResistPerLevel { get; }
        float HealthRegenerationPerLevel { get; }
        float ManaRegenerationPerLevel { get; }
        float GrowthAttackSpeed { get; set; }
        float[] ManaCost { get; }
        IStat AbilityPower { get; }
        IStat Armor { get; }
        IStat ArmorPenetration { get; }
        IStat AttackDamage { get; }
        IStat AttackDamagePerLevel { get; }
        IStat AttackSpeedMultiplier { get; }
        IStat CooldownReduction { get; }
        IStat CriticalChance { get; }
        IStat CriticalDamage { get; }
        IStat ExpGivenOnDeath { get; }
        IStat GoldPerGoldTick { get; }
        IStat GoldGivenOnDeath { get; }
        IStat HealthPoints { get; }
        IStat HealthRegeneration { get; }
        IStat LifeSteal { get; }
        IStat MagicResist { get; }
        IStat MagicPenetration { get; }
        IStat ManaPoints { get; }
        IStat ManaRegeneration { get; }
        IStat MoveSpeed { get; }
        IStat Range { get; }
        IStat Size { get; }
        IStat SpellVamp { get; }
        IStat Tenacity { get; }
        IStat AcquisitionRange { get; }
        float Gold { get; set; }
        byte Level { get; set; }
        float Experience { get; set; }
        float Points { get; set; }
        float CurrentHealth { get; set; }
        float CurrentMana { get; set; }
        bool IsGeneratingGold { get; set; }
        float SpellCostReduction { get; }
        
        void AddModifier(IStatsModifier modifier);
        void RemoveModifier(IStatsModifier modifier);
        void LevelUp();
        
        float GetTotalAttackSpeed();
        bool GetActionState(ActionState state);
        bool GetSpellEnabled(byte id);
        float GetPostMitigationDamage(float damage, DamageType type, IAttackableUnit attacker);
        bool GetSummonerSpellEnabled(byte id);
        
        void SetActionState(ActionState state, bool enabled);
        void SetSpellEnabled(byte id, bool enabled);
        void SetSummonerSpellEnabled(byte id, bool enabled);
    }
}
