using GameServerCore.Enums;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Domain.GameObjects.Spell.Missile
{
    public interface ISpellMissile : IGameObject
    {
        /// <summary>
        /// Information about this missile's path.
        /// </summary>
        ICastInfo CastInfo { get; }
        /// <summary>
        /// What kind of behavior this missile has.
        /// </summary>
        MissileType Type { get; }
        /// <summary>
        /// Current unit this projectile is homing in on and moving towards. Projectile is destroyed on contact with this unit unless it has more than one target.
        /// </summary>
        IAttackableUnit TargetUnit { get; }
        /// <summary>
        /// Spell which created this projectile.
        /// </summary>
        ISpell SpellOrigin { get; }
        /// <summary>
        /// Whether or not this projectile's visuals should not be networked to clients.
        /// </summary>
        bool IsServerOnly { get; }

        /// <summary>
        /// Gets the server-side speed that this Projectile moves at in units/sec.
        /// </summary>
        /// <returns>Units travelled per second.</returns>
        float GetSpeed();
        /// <summary>
        /// Gets the time since this projectile was created.
        /// </summary>
        /// <returns></returns>
        float GetTimeSinceCreation();
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
