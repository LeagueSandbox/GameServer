using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects.Spell.Missile
{
    public interface ISectorParameters
    {
        bool CanHitSameTarget { get; }
        bool CanHitSameTargetConsecutively { get; }

        int MaximumHits { get; }

        SectorType Type { get; }
    }
}