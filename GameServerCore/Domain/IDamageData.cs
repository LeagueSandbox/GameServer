using GameServerCore.Domain.GameObjects;
using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface IDamageData
    {
        /// <summary>
        /// Wheter or not the damage came from an auttoatack or a Spell
        /// </summary>
        bool IsAutoAttack { get; }
        /// <summary>
        /// Unit that received the damage.
        /// </summary>
        IAttackableUnit Target { get; }
        /// <summary>
        /// Unit that inflicted the damage.
        /// </summary>
        IAttackableUnit Attacker { get; }
        /// <summary>
        /// Type of damage received.
        /// </summary>
        DamageType DamageType { get; }
        /// <summary>
        /// Source of the damage.
        /// </summary>
        DamageSource DamageSource { get; }
        /// <summary>
        /// The raw ammount of damage to be inflicted (Pre-Mitigation damage)
        /// </summary>
        float Damage { get; }
        /// <summary>
        /// Damage after being reduced by MR/Armor
        /// </summary>
        float PostMitigationdDamage { get; }
    }
}