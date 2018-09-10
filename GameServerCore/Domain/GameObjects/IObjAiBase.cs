using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface IObjAiBase : IAttackableUnit
    {
        IAttackableUnit TargetUnit { get; }
        IAttackableUnit AutoAttackTarget { get; }

        float AutoAttackDelay { get; }
        float AutoAttackProjectileSpeed { get; }
        MoveOrder MoveOrder { get; }
        bool IsCastingSpell { get; }
        bool IsMelee { get; }
        void UpdateTargetUnit(IAttackableUnit unit);
        void StopMovement();
    }
}
