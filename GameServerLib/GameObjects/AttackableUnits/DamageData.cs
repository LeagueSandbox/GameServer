using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerLib.GameObjects.AttackableUnits
{
    class DamageData : IDamageData
    {
        /// <summary>
        /// Wheter or not the damage came from an auttoatack or a Spell
        /// </summary>
        public bool IsAutoAttack { get; set; }
        /// <summary>
        /// Unit that received the damage.
        /// </summary>
        public IAttackableUnit Target { get; set; }
        /// <summary>
        /// Unit that inflicted the damage.
        /// </summary>
        public IAttackableUnit Attacker { get; set; }
        /// <summary>
        /// Type of damage received.
        /// </summary>
        public DamageType DamageType { get; set; }
        /// <summary>
        /// Source of the damage.
        /// </summary>
        public DamageSource DamageSource { get; set; }
        /// <summary>
        /// The raw ammount of damage to be inflicted (Pre-mitigated damage)
        /// </summary>
        public float Damage { get; set; }
        /// <summary>
        /// Mitigated ammount of damage (after being reduced by armor/MR stats) 
        /// </summary>
        public float PostMitigationdDamage { get; set; }
    }
}