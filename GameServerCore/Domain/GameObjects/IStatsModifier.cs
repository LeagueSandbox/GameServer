namespace GameServerCore.Domain.GameObjects
{
    public interface IStatsModifier
    {
        IStatModifier AbilityPower { get; }
        IStatModifier AcquisitionRange { get; }
        IStatModifier Armor { get; }
        IStatModifier ArmorPenetration { get; }
        IStatModifier AttackDamage { get; }
        IStatModifier AttackDamagePerLevel { get; }
        IStatModifier AttackSpeed { get; }
        IStatModifier CooldownReduction { get; }
        IStatModifier CriticalChance { get; }
        IStatModifier CriticalDamage { get; }
        IStatModifier GoldGivenOnDeath { get; }
        IStatModifier GoldPerSecond { get; }
        IStatModifier HealthPoints { get; }
        IStatModifier HealthRegeneration { get; }
        IStatModifier LifeSteal { get; }
        IStatModifier MagicPenetration { get; }
        IStatModifier MagicResist { get; }
        IStatModifier ManaPoints { get; }
        IStatModifier ManaRegeneration { get; }
        IStatModifier MoveSpeed { get; }
        IStatModifier Range { get; }
        IStatModifier Size { get; }
        IStatModifier SpellVamp { get; }
        IStatModifier Tenacity { get; }
    }
}
