using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Domain.GameObjects
{
    public interface IProjectile : IObjMissile
    {
        /// <summary>
        /// Number of objects this projectile has hit since it was created.
        /// </summary>
        List<IGameObject> ObjectsHit { get; }
        /// <summary>
        /// Unit which owns the spell that created this projectile.
        /// </summary>
        IAttackableUnit Owner { get; }
        /// <summary>
        /// Unique identification of this projectile.
        /// </summary>
        int ProjectileId { get; }
        /// <summary>
        /// Projectile spell data, housing all information about this projectile's properties. Most projectiles are counted as ExtraSpells within a character's data.
        /// </summary>
        ISpellData SpellData { get; }
        /// <summary>
        /// Current unit this projectile is homing in on and moving towards. Projectile is destroyed on contact with this unit.
        /// </summary>
        IAttackableUnit TargetUnit { get; }
        /// <summary>
        /// Position this projectile is moving towards. Projectile is destroyed once it reaches this destination. Equals Vector2.Zero if TargetUnit is not null.
        /// </summary>
        Vector2 Destination { get; }
        /// <summary>
        /// Spell which created this projectile.
        /// </summary>
        ISpell OriginSpell { get; }
        /// <summary>
        /// Whether or not this projectile's visuals should not be networked to clients.
        /// </summary>
        bool IsServerOnly { get; }

        /// <summary>
        /// Whether or not this projectile has a target unit or a destination; if it is a valid projectile.
        /// </summary>
        /// <returns>True/False.</returns>
        bool HasTarget();
        /// <summary>
        /// Gets the position of this projectile's target (unit or destination).
        /// </summary>
        /// <returns>Vector2 position of target. Vector2(float.NegativeInfinity, float.NegativeInfinity) if projectile has no target.</returns>
        Vector2 GetTargetPosition();
    }
}
