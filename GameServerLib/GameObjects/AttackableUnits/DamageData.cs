using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerLib.GameObjects.AttackableUnits
{
    class DamageData : IDamageData
    {
        /// <summary>
        /// Unit that inflicted the damage.
        /// </summary>
        public IAttackableUnit Attacker { get; set; }
        /// <summary>
        /// The raw amount of damage to be inflicted (Pre-mitigated damage)
        /// </summary>
        public float Damage { get; set; }
        /// <summary>
        /// The result of this damage (Ex. Dodged, Missed, Invulnerable or Crit)
        /// </summary>
        public DamageResultType DamageResultType { get; set; } = DamageResultType.RESULT_NORMAL;
        /// <summary>
        /// Source of the damage.
        /// </summary>
        public DamageSource DamageSource { get; set; }
        /// <summary>
        /// Type of damage received.
        /// </summary>
        public DamageType DamageType { get; set; }
        /// <summary>
        /// Whether or not the damage came from an autoatack or a Spell
        /// </summary>
        public bool IsAutoAttack { get; set; }
        /// <summary>
        /// Mitigated amount of damage (after being reduced by Armor/MR stats) 
        /// </summary>
        public float PostMitigationDamage { get; set; }
        /// <summary>
        /// Unit that will receive the damage.
        /// </summary>
        public IAttackableUnit Target { get; set; }
    }
}
