﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServerCore.Logic.Domain.GameObjects
{
    public interface IStatsModifier
    {
        IStatModifier HealthPoints { get; }
        IStatModifier HealthRegeneration { get; }
        IStatModifier AttackDamage { get; }
        IStatModifier AbilityPower { get; }
        IStatModifier CriticalChance { get; }
        IStatModifier CriticalDamage { get; }
        IStatModifier Armor { get; }
        IStatModifier MagicResist { get; }
        IStatModifier AttackSpeed { get; }
        IStatModifier ArmorPenetration { get; }
        IStatModifier MagicPenetration { get; }
        IStatModifier ManaPoints { get; }
        IStatModifier ManaRegeneration { get; }
        IStatModifier LifeSteel { get; }
        IStatModifier SpellVamp { get; }
        IStatModifier Tenacity { get; }
        IStatModifier Size { get; }
        IStatModifier Range { get; }
        IStatModifier MoveSpeed { get; }
        IStatModifier GoldPerSecond { get; }
    }
}
