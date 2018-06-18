using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class ReplicationMinion : Replication
    {
        public ReplicationMinion(Minion owner) : base(owner)
        {

        }
        public override void Update()
        {
            UpdateFloat(Stats.CurrentHealth, 1, 0);
            UpdateFloat(Stats.HealthPoints.Total, 1, 1);
            // UpdateFloat(Stats.LifeTime, 1, 2);
            // UpdateFloat(Stats.MaxLifeTime, 1, 3);
            // UpdateFloat(Stats.LifeTimeTicks, 1, 4);
            UpdateFloat(Stats.ManaPoints.Total, 1, 5);
            UpdateFloat(Stats.CurrentMana, 1, 6);
            // UpdateUint((uint)Stats.ActionState, 1, 7);
            // UpdateBool(Stats.IsMagicImmune, 1, 8);
            // UpdateBool(Stats.IsInvulnerable, 1, 9);
            // UpdateBool(Stats.IsPhysicalImmune, 1, 10);
            // UpdateBool(Stats.IsLifestealImmune, 1, 11);
            UpdateFloat(Stats.AttackDamage.BaseValue, 1, 12);
            UpdateFloat(Stats.Armor.Total, 1, 13);
            UpdateFloat(Stats.MagicResist.Total, 1, 14);
            UpdateFloat(Stats.GetTotalAttackSpeed(), 1, 15);
            UpdateFloat(Stats.AttackDamage.FlatBonus, 1, 16);
            UpdateFloat(Stats.AttackDamage.PercentBonus, 1, 17);
            UpdateFloat(Stats.AbilityPower.Total, 1, 18);
            UpdateFloat(Stats.HealthRegeneration.Total, 1, 19);
            UpdateFloat(Stats.ManaRegeneration.Total, 1, 20);
            UpdateFloat(Stats.MagicResist.FlatBonus, 1, 21);
            UpdateFloat(Stats.MagicResist.PercentBonus, 1, 22);
            // UpdateFloat(Stats.PerceptionRange.FlatBonus, 3, 0);
            // UpdateFloat(Stats.PerceptionRange.PercentBonus, 3, 1);
            UpdateFloat(Stats.MoveSpeed.Total, 3, 2);
            UpdateFloat(Stats.Size.Total, 3, 3);
            // UpdateBool(Stats.IsTargetable, 3, 4);
            // UpdateUint((uint)Stats.IsTargetableToTeam, 3, 5);
        }
    }
}
