using System.Collections.Generic;

namespace GameServerCore.Domain.GameObjects.Spell.Sector
{
    public interface ISpellSector : IGameObject
    {
        /// <summary>
        /// Information about the creation of this sector.
        /// </summary>
        ICastInfo CastInfo { get; }
        /// <summary>
        /// Spell which created this projectile.
        /// </summary>
        ISpell SpellOrigin { get; }
        /// <summary>
        /// Parameters for this sector, refer to ISectorParameters.
        /// </summary>
        ISectorParameters Parameters { get; }
        /// <summary>
        /// All objects this sector has hit since it was created and how many times each has been hit.
        /// </summary>
        List<IGameObject> ObjectsHit { get; }
        /// <summary>
        /// Total number of times this sector has hit any units.
        /// </summary>
        /// TODO: Verify if we want this to be an array for different MaximumHit counts for: CanHitCaster, CanHitEnemies, CanHitFriends, CanHitSameTarget, and CanHitSameTargetConsecutively.
        int HitCount { get; }

        /// <summary>
        /// Forces this spell sector to perform a tick.
        /// </summary>
        void ExecuteTick();
    }
}
