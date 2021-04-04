using GameServerCore.Enums;
using System.Collections.Generic;
using System.Numerics;

namespace GameServerCore.Domain.GameObjects.Spell.Missile
{
    public interface ISpellChainMissile : ISpellMissile
    {
        /// <summary>
        /// Number of objects this projectile has hit since it was created.
        /// </summary>
        List<IGameObject> ObjectsHit { get; }
        /// <summary>
        /// Total number of times this missile has hit any units.
        /// </summary>
        int HitCount { get; }
        /// <summary>
        /// Parameters for this chain missile, refer to IMissileParameters.
        /// </summary>
        IMissileParameters Parameters { get; }

        void CheckFlagsForUnit(IAttackableUnit unit);
    }
}