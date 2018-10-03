using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface IObjAiBase : IAttackableUnit
    {
        IAttackableUnit TargetUnit { get; set;  }
        IAttackableUnit AutoAttackTarget { get; }

        float AutoAttackDelay { get; }
        float AutoAttackProjectileSpeed { get; }
        MoveOrder MoveOrder { get; }
        bool IsCastingSpell { get; set;  }
        bool IsMelee { get; }
        void UpdateTargetUnit(IAttackableUnit unit);
        void StopMovement();
        void TeleportTo(float x, float y);
        void AddStatModifier(IStatsModifier statModifier);
        void RemoveStatModifier(IStatsModifier statModifier);

        // buffs
        bool HasBuffGameScriptActive(string buffNamespace, string buffClass);
        void AddBuffGameScript(string buffNamespace, string buffClass, ISpell ownerSpell, float removeAfter = -1f, bool isUnique = false);
        void RemoveBuffSlot(IBuff b);
        byte GetNewBuffSlot(IBuff b);
        void AddBuff(IBuff b);
        void ApplyCrowdControl(ICrowdControl cc);
        void RemoveCrowdControl(ICrowdControl cc);
    }
}
