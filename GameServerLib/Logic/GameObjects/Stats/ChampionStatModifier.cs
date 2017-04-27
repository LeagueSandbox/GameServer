using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class ChampionStatModifier : IStatsModifier
    {
        // Stats
        public StatModifcator HealthPoints { get; set; }
        public StatModifcator HealthRegeneration { get; set; }
        public StatModifcator AttackDamage { get; set; }
        public StatModifcator AbilityPower { get; set; }
        public StatModifcator CriticalChance { get; set; }
        public StatModifcator Armor { get; set; }
        public StatModifcator MagicResist { get; set; }
        public StatModifcator AttackSpeed { get; set; }
        public StatModifcator ArmorPenetration { get; set; }
        public StatModifcator MagicPenetration { get; set; }
        public StatModifcator ManaPoints { get; set; }
        public StatModifcator ManaRegeneration { get; set; }
        public StatModifcator LifeSteel { get; set; }
        public StatModifcator SpellVamp { get; set; }
        public StatModifcator Tenacity { get; set; }
        public StatModifcator Size { get; set; }
        public StatModifcator Range { get; set; }
        public StatModifcator MoveSpeed { get; set; }
        public StatModifcator GoldPerSecond { get; set; }

        public ChampionStatModifier()
        {
            HealthPoints = new StatModifcator();
            HealthRegeneration = new StatModifcator();
            AttackDamage = new StatModifcator();
            AbilityPower = new StatModifcator();
            CriticalChance = new StatModifcator();
            Armor = new StatModifcator();
            MagicResist = new StatModifcator();
            AttackSpeed = new StatModifcator();
            ArmorPenetration = new StatModifcator();
            MagicPenetration = new StatModifcator();
            ManaPoints = new StatModifcator();
            ManaRegeneration = new StatModifcator();
            LifeSteel = new StatModifcator();
            SpellVamp = new StatModifcator();
            Tenacity = new StatModifcator();
            Size = new StatModifcator();
            Range = new StatModifcator();
            MoveSpeed = new StatModifcator();
            GoldPerSecond = new StatModifcator();
        }
    }
}
