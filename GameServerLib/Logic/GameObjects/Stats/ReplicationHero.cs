using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class ReplicationHero : Replication
    {
        public ReplicationHero(Champion owner) : base(owner)
        {

        }
        public override void Update()
        {
            UpdateFloat(Stats.Gold, 0, 0);
            // UpdateFloat(Stats.TotalGold, 0, 1);
            UpdateUint((UInt32)(Stats.SpellsEnabled), 0, 2);
            UpdateUint((UInt32)(Stats.SpellsEnabled >> 32), 0, 3);
            UpdateUint((UInt32)(Stats.SummonerSpellsEnabled), 0, 4);
            UpdateUint((UInt32)(Stats.SummonerSpellsEnabled >> 32), 0, 5);
            // UpdateUint(Stats.EvolvePoints, 0, 6);
            // UpdateUint(Stats.EvolveFlags, 0, 7);
            for(var i = 0; i < 4; i++)
            {
                UpdateFloat(Stats.ManaCost[i], 0, 8 + i);
            }
            for(var i = 0; i < 16; i++)
            {
                UpdateFloat(Stats.ManaCost[45 + i], 0, 12 + i);
            }
            // UpdateUint((uint)Stats.ActionState, 1, 0);
            // UpdateBool(Stats.IsMagicImmune, 1, 1);
            // UpdateBool(Stats.IsInvulnerable, 1, 2);
            // UpdateBool(Stats.IsPhysicalImmune, 1, 3);
            // UpdateBool(Stats.IsLifestealImmune, 1, 4);
            UpdateFloat(Stats.AttackDamage.BaseValue, 1, 5);
            // UpdateFloat(Stats.AbilityPower.BaseValue, 1, 6);
            // UpdateFloat(Stats.DodgeChance, 1, 7);
            UpdateFloat(Stats.CriticalChance.Total, 1, 8);
            UpdateFloat(Stats.Armor.Total, 1, 9);
            UpdateFloat(Stats.MagicResist.Total, 1, 10);
            UpdateFloat(Stats.HealthRegeneration.Total, 1, 11);
            UpdateFloat(Stats.ManaRegeneration.Total, 1, 12);
            UpdateFloat(Stats.Range.Total, 1, 13);
            UpdateFloat(Stats.AttackDamage.FlatBonus, 1, 14);
            UpdateFloat(Stats.AttackDamage.PercentBonus, 1, 15);
            UpdateFloat(Stats.AbilityPower.FlatBonus, 1, 16);
            UpdateFloat(Stats.MagicResist.FlatBonus, 1, 17);
            UpdateFloat(Stats.MagicResist.PercentBonus, 1, 18);
            UpdateFloat(Stats.GetTotalAttackSpeed(), 1, 19);
            UpdateFloat(Stats.Range.FlatBonus, 1, 20);
            UpdateFloat(Stats.CooldownReduction.Total, 1, 21);
            // UpdateFloat(Stats.PassiveCooldownEndTime, 1, 22);
            // UpdateFloat(Stats.PassiveCooldownTotalTime, 1, 23);
            UpdateFloat(Stats.ArmorPenetration.FlatBonus, 1, 24);
            UpdateFloat(Stats.ArmorPenetration.PercentBaseBonus, 1, 25);
            UpdateFloat(Stats.MagicPenetration.FlatBonus, 1, 26);
            UpdateFloat(Stats.MagicPenetration.PercentBaseBonus, 1, 27);
            UpdateFloat(Stats.LifeSteal.Total, 1, 28);
            UpdateFloat(Stats.SpellVamp.Total, 1, 29);
            UpdateFloat(Stats.Tenacity.Total, 1, 30);
            UpdateFloat(Stats.Armor.PercentBonus, 2, 0);
            UpdateFloat(Stats.MagicPenetration.PercentBonus, 2, 1);
            UpdateFloat(Stats.HealthRegeneration.BaseValue, 2, 2);
            UpdateFloat(Stats.ManaRegeneration.BaseValue, 2, 3);
            UpdateFloat(Stats.CurrentHealth, 3, 0);
            UpdateFloat(Stats.CurrentMana, 3, 1);
            UpdateFloat(Stats.HealthPoints.Total, 3, 2);
            UpdateFloat(Stats.ManaPoints.Total, 3, 3);
            UpdateFloat(Stats.Experience, 3, 4);
            // UpdateFloat(Stats.LifeTime, 3, 5);
            // UpdateFloat(Stats.MaxLifeTime, 3, 6);
            // UpdateFloat(Stats.LifeTimeTicks, 3, 7);
            // UpdateFloat(Stats.PerceptionRange.FlatMod, 3, 8);
            // UpdateFloat(Stats.PerceptionRange.PercentMod, 3, 9);
            UpdateFloat(Stats.MoveSpeed.Total, 3, 10);
            UpdateFloat(Stats.Size.Total, 3, 11);
            // UpdateFloat(Stats.FlatPathfindingRadiusMod, 3, 12);
            UpdateUint(Stats.Level, 3, 13);
            // UpdateUint(Stats.NumberOfNeutralMinionsKilled, 3, 14);
            // UpdateBool(Stats.IsTargetable, 3, 15);
            // UpdateUint((uint)Stats.IsTargetableToTeam, 3, 16);
        }
    }
}
