using GameServerCore.Domain.GameObjects;

namespace LeagueSandbox.GameServer.GameObjects.Stats
{
    public class StatsModifier : IStatsModifier
    {
        // Stats
        public IStatModifier HealthPoints { get; set; } = new StatModifier();
        public IStatModifier HealthRegeneration { get; set; } = new StatModifier();
        public IStatModifier AttackDamage { get; set; } = new StatModifier();
        public IStatModifier AbilityPower { get; set; } = new StatModifier();
        public IStatModifier CriticalChance { get; set; } = new StatModifier();
        public IStatModifier CriticalDamage { get; set; } = new StatModifier();
        public IStatModifier Armor { get; set; } = new StatModifier();
        public IStatModifier MagicResist { get; set; } = new StatModifier();
        public IStatModifier AttackSpeed { get; set; } = new StatModifier();
        public IStatModifier ArmorPenetration { get; set; } = new StatModifier();
        public IStatModifier MagicPenetration { get; set; } = new StatModifier();
        public IStatModifier ManaPoints { get; set; } = new StatModifier();
        public IStatModifier ManaRegeneration { get; set; } = new StatModifier();
        public IStatModifier LifeSteal { get; set; } = new StatModifier();
        public IStatModifier SpellVamp { get; set; } = new StatModifier();
        public IStatModifier Tenacity { get; set; } = new StatModifier();
        public IStatModifier Size { get; set; } = new StatModifier();
        public IStatModifier Range { get; set; } = new StatModifier();
        public IStatModifier MoveSpeed { get; set; } = new StatModifier();
        public IStatModifier GoldPerSecond { get; set; } = new StatModifier();
    }
}
