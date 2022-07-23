
namespace LeagueSandbox.GameServer.GameObjects.StatsNS
{
    public class StatsModifier
    {
        // Stats
        public StatModifier AbilityPower { get; } = new StatModifier();
        public StatModifier AcquisitionRange { get; } = new StatModifier();
        public StatModifier Armor { get; } = new StatModifier();
        public StatModifier ArmorPenetration { get; } = new StatModifier();
        public StatModifier AttackDamage { get; } = new StatModifier();
        public StatModifier AttackDamagePerLevel { get; } = new StatModifier();
        public StatModifier AttackSpeed { get; } = new StatModifier();
        public StatModifier CooldownReduction { get; } = new StatModifier();
        public StatModifier CriticalChance { get; } = new StatModifier();
        public StatModifier CriticalDamage { get; } = new StatModifier();
        public StatModifier GoldGivenOnDeath { get; } = new StatModifier();
        public StatModifier GoldPerSecond { get; } = new StatModifier();
        public StatModifier HealthPoints { get; } = new StatModifier();
        public StatModifier HealthRegeneration { get; } = new StatModifier();
        public StatModifier LifeSteal { get; } = new StatModifier();
        public StatModifier MagicPenetration { get; } = new StatModifier();
        public StatModifier MagicResist { get; } = new StatModifier();
        public StatModifier ManaPoints { get; } = new StatModifier();
        public StatModifier ManaRegeneration { get; } = new StatModifier();
        public StatModifier MoveSpeed { get; } = new StatModifier();
        public StatModifier Range { get; } = new StatModifier();
        public StatModifier Size { get; } = new StatModifier();
        public StatModifier SpellVamp { get; } = new StatModifier();
        public StatModifier Tenacity { get; } = new StatModifier();
        public float MultiplicativeSpeedBonus { get; set; }
        public float SlowResistPercent { get; set; }
    }
}
