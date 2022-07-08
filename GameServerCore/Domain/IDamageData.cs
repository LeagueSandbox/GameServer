using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface IDamageData
    {
        /// <summary>
        /// Unit that inflicted the damage.
        /// </summary>
        IAttackableUnit Attacker { get; }
        /// <summary>
        /// The raw amount of damage to be inflicted (Pre-Mitigation damage)
        /// </summary>
        float Damage { get; }
        /// <summary>
        /// The result of this damage (Ex. Dodged, Missed, Invulnerable or Crit)
        /// </summary>
        DamageResultType DamageResultType { get; set; }
        /// <summary>
        /// Source of the damage.
        /// </summary>
        DamageSource DamageSource { get; }
        /// <summary>
        /// Type of damage received.
        /// </summary>
        DamageType DamageType { get; }

        /// <summary>
        /// Whether or not the damage came from an autoatack or a Spell
        /// </summary>
        bool IsAutoAttack { get; }
        /// <summary>
        /// Damage after being reduced by MR/Armor
        /// </summary>
        float PostMitigationDamage { get; }
        /// <summary>
        /// Unit that received the damage.
        /// </summary>
        IAttackableUnit Target { get; }
    }
}
