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

        // buffs
        bool HasBuffGameScriptActive(string buffNamespace, string buffClass);
        void AddBuffGameScript(string buffNamespace, string buffClass, ISpell ownerSpell, float removeAfter = -1f, bool isUnique = false);
    }
}
