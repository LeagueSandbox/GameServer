using GameServerCore.Domain.GameObjects;

namespace LeagueSandbox.GameServer.GameObjects.Stats
{
    public class StatsModifier : IStatsModifier
    {
        // Stats
        public IStatModifier AbilityPower { get; } = new StatModifier();
        public IStatModifier AcquisitionRange { get; } = new StatModifier();
        public IStatModifier Armor { get; } = new StatModifier();
        public IStatModifier ArmorPenetration { get; } = new StatModifier();
        public IStatModifier AttackDamage { get; } = new StatModifier();
        public IStatModifier AttackDamagePerLevel { get; } = new StatModifier();
        public IStatModifier AttackSpeed { get; } = new StatModifier();
        public IStatModifier CooldownReduction { get; } = new StatModifier();
        public IStatModifier CriticalChance { get; } = new StatModifier();
        public IStatModifier CriticalDamage { get; } = new StatModifier();
        public IStatModifier GoldGivenOnDeath { get; } = new StatModifier();
        public IStatModifier GoldPerSecond { get; } = new StatModifier();
        public IStatModifier HealthPoints { get; } = new StatModifier();
        public IStatModifier HealthRegeneration { get; } = new StatModifier();
        public IStatModifier LifeSteal { get; } = new StatModifier();
        public IStatModifier MagicPenetration { get; } = new StatModifier();
        public IStatModifier MagicResist { get; } = new StatModifier();
        public IStatModifier ManaPoints { get; } = new StatModifier();
        public IStatModifier ManaRegeneration { get; } = new StatModifier();
        public IStatModifier MoveSpeed { get; } = new StatModifier();
        public IStatModifier Range { get; } = new StatModifier();
        public IStatModifier Size { get; } = new StatModifier();
        public IStatModifier SpellVamp { get; } = new StatModifier();
        public IStatModifier Tenacity { get; } = new StatModifier();
    }
}
