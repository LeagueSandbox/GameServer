using System.Collections.Generic;

namespace GameServerCore.Domain.GameObjects
{
    public interface IProjectile : IObjMissile
    {
        List<IGameObject> ObjectsHit { get; }
        IAttackableUnit Owner { get; }
        int ProjectileId { get; }
        ISpellData SpellData { get; }
        ISpell OriginSpell { get; }
        bool IsServerOnly { get; }
    }
}
