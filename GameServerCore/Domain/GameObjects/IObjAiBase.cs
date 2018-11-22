using System.Collections.Generic;
using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface IObjAiBase : IAttackableUnit
    {
        IAttackableUnit TargetUnit { get; set;  }
        IAttackableUnit AutoAttackTarget { get; set; }
        bool IsAttacking { get; set; } 
        bool IsDashing { get; }
        bool HasMadeInitialAttack { get; set; }

        float AutoAttackDelay { get; set;  }
        float AutoAttackProjectileSpeed { get; set;  }
        MoveOrder MoveOrder { get; }
        bool IsCastingSpell { get; set;  }
        bool IsMelee { get; set; }
        void UpdateTargetUnit(IAttackableUnit unit);
        void StopMovement();
        void TeleportTo(float x, float y);
        void AddStatModifier(IStatsModifier statModifier);
        void RemoveStatModifier(IStatsModifier statModifier);
        void SetTargetUnit(IAttackableUnit target);
        void AutoAttackHit(IAttackableUnit target);

        // buffs
        bool HasBuffGameScriptActive(string buffNamespace, string buffClass);
        void AddBuffGameScript(string buffNamespace, string buffClass, ISpell ownerSpell, float removeAfter = -1f, bool isUnique = false);
        void RemoveBuffSlot(IBuff b);
        byte GetNewBuffSlot(IBuff b);
        void AddBuff(IBuff b);
        void ApplyCrowdControl(ICrowdControl cc);
        void RemoveCrowdControl(ICrowdControl cc);
        void SetDashingState(bool state);
        void DashToTarget(ITarget t, float dashSpeed, float followTargetMaxDistance, float backDistance, float travelTime);
        Dictionary<string, IBuff> GetBuffs();
        void RemoveBuff(IBuff b);
    }
}
