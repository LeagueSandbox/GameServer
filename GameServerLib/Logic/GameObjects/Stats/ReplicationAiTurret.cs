using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class ReplicationAiTurret : Replication
    {
        public ReplicationAiTurret(BaseTurret owner) : base(owner)
        {

        }
        public override void Update()
        {
            UpdateFloat(Stats.ManaPoints.Total, 1, 0);
            UpdateFloat(Stats.CurrentMana, 1, 1);
            // UpdateUint((uint)Stats.ActionState, 1, 2);
            // UpdateBool(Stats.IsMagicImmune, 1, 3);
            // UpdateBool(Stats.IsInvulnerable, 1, 4);
            // UpdateBool(Stats.IsPhysicalImmune, 1, 5);
            // UpdateBool(Stats.IsLifestealImmune, 1, 6);
            UpdateFloat(Stats.AttackDamage.BaseValue, 1, 7);
            UpdateFloat(Stats.Armor.Total, 1, 9);
            UpdateFloat(Stats.MagicResist.Total, 1, 10);
            UpdateFloat(Stats.GetTotalAttackSpeed(), 1, 11);
            UpdateFloat(Stats.AttackDamage.FlatBonus, 1, 12);
            UpdateFloat(Stats.AttackDamage.PercentBonus, 1, 13);
            UpdateFloat(Stats.AbilityPower.Total, 1, 14);
            UpdateFloat(Stats.HealthRegeneration.Total, 1, 15);
            UpdateFloat(Stats.CurrentHealth, 3, 0);
            UpdateFloat(Stats.HealthPoints.Total, 3, 1);
            // UpdateFloat(Stats.PerceptionRange.FlatBonus, 3, 2);
            // UpdateFloat(Stats.PerceptionRange.PercentBonus, 3, 3);
            UpdateFloat(Stats.MoveSpeed.Total, 3, 4);
            UpdateFloat(Stats.Size.Total, 3, 5);
            // UpdateBool(Stats.IsTargetable, 5, 0);
            // UpdateUint((uint)Stats.IsTargetableToTeam, 5, 1);
        }
    }
}
