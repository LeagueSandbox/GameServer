using System.Collections.Generic;

namespace GameServerCore.Domain.GameObjects.Spell.Missile
{
    public interface ISpellChainMissile : ISpellMissile // TODO: Change to ISpellMissile for spells rework.
    {
        /// <summary>
        /// Number of objects this projectile has hit since it was created.
        /// </summary>
        List<IGameObject> ObjectsHit { get; }
        /// <summary>
        /// Parameters for this chain missile, refer to IMissileParameters.
        /// </summary>
        IMissileParameters Parameters { get; }

        void CheckFlagsForUnit(IAttackableUnit unit);
    }
}