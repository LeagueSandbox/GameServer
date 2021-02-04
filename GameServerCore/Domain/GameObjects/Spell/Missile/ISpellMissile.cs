using GameServerCore.Domain.GameObjects.Spell;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Domain.GameObjects.Spell.Missile
{
    public interface ISpellMissile : IGameObject
    {
        /// <summary>
        /// Number of objects this projectile has hit since it was created.
        /// </summary>
        List<IGameObject> ObjectsHit { get; }
        /// <summary>
        /// Projectile spell data, housing all information about this projectile's properties. Most projectiles are counted as ExtraSpells within a character's data.
        /// </summary>
        ISpellData SpellData { get; }
        ICastInfo CastInfo { get; }
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
