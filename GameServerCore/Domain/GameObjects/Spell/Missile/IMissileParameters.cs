using GameServerCore.Enums;
using System.Numerics;

namespace GameServerCore.Domain.GameObjects.Spell.Missile
{
    public interface IMissileParameters
    {
        bool CanHitSameTarget { get; }
        bool CanHitSameTargetConsecutively { get; }

        int MaximumHits { get; }

        MissileType Type { get; }
        Vector2 OverrideEndPosition { get; }
    }
}