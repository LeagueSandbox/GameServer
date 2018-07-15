namespace LeagueSandbox.GameServer.Logic.GameObjects.Stats
{
    public class StatsModifier
    {
        // Stats
        public StatModifier HealthPoints { get; set; } = new StatModifier();
        public StatModifier HealthRegeneration { get; set; } = new StatModifier();
        public StatModifier AttackDamage { get; set; } = new StatModifier();
        public StatModifier AbilityPower { get; set; } = new StatModifier();
        public StatModifier CriticalChance { get; set; } = new StatModifier();
        public StatModifier CriticalDamage { get; set; } = new StatModifier();
        public StatModifier Armor { get; set; } = new StatModifier();
        public StatModifier MagicResist { get; set; } = new StatModifier();
        public StatModifier AttackSpeed { get; set; } = new StatModifier();
        public StatModifier ArmorPenetration { get; set; } = new StatModifier();
        public StatModifier MagicPenetration { get; set; } = new StatModifier();
        public StatModifier ManaPoints { get; set; } = new StatModifier();
        public StatModifier ManaRegeneration { get; set; } = new StatModifier();
        public StatModifier LifeSteel { get; set; } = new StatModifier();
        public StatModifier SpellVamp { get; set; } = new StatModifier();
        public StatModifier Tenacity { get; set; } = new StatModifier();
        public StatModifier Size { get; set; } = new StatModifier();
        public StatModifier Range { get; set; } = new StatModifier();
        public StatModifier MoveSpeed { get; set; } = new StatModifier();
        public StatModifier GoldPerSecond { get; set; } = new StatModifier();
    }
}
