﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public interface IBuff
    {
        StatModifcator HealthPoints { get; set; }
        StatModifcator HealthRegeneration { get; set; }
        StatModifcator AttackDamage { get; set; }
        StatModifcator AbilityPower { get; set; }
        StatModifcator CriticalChance { get; set; }
        StatModifcator Armor { get; set; }
        StatModifcator MagicResist { get; set; }
        StatModifcator AttackSpeed { get; set; }
        StatModifcator ArmorPenetration { get; set; }
        StatModifcator MagicPenetration { get; set; }
        StatModifcator ManaPoints { get; set; }
        StatModifcator ManaRegeneration{ get; set; }
        StatModifcator LifeSteel { get; set; }
        StatModifcator SpellVamp { get; set; }
        StatModifcator Tenacity { get; set; }
        StatModifcator Size { get; set; }
        StatModifcator Range { get; set; }
        StatModifcator MoveSpeed { get; set; }
        StatModifcator GoldPerSecond { get; set; }
    }
}
