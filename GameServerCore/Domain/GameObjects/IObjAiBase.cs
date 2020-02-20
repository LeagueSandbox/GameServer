using System.Collections.Generic;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;

namespace GameServerCore.Domain.GameObjects
{
    public interface IObjAiBase : IAttackableUnit
    {
        ISpellData AaSpellData { get; }
        float AutoAttackCastTime { get; set; }
        float AutoAttackProjectileSpeed { get; set; }
        IAttackableUnit AutoAttackTarget { get; set; }
        ICharData CharData { get; }
        float DashSpeed { get; set; }
        bool HasMadeInitialAttack { get; set; }
        IInventoryManager Inventory { get; }
        bool IsAttacking { get; set; }
        bool IsCastingSpell { get; set; }
        bool IsDashing { get; }
        bool IsMelee { get; set; }
        MoveOrder MoveOrder { get; }
        IAttackableUnit TargetUnit { get; set;  }

        void AddBuff(IBuff b);
        void AddStatModifier(IStatsModifier statModifier);
        void ApplyCrowdControl(ICrowdControl cc);
        void AutoAttackHit(IAttackableUnit target);
        void ChangeAutoAttackSpellData(string newAutoAttackSpellDataName);
        void ChangeAutoAttackSpellData(ISpellData newAutoAttackSpellData);
        void DashToTarget(ITarget t, float dashSpeed, float followTargetMaxDistance, float backDistance, float travelTime);
        IBuff GetBuffWithName(string name);
        int GetBuffsCount();
        List<IBuff> GetBuffs();
        List<IBuff> GetBuffsWithName(string buffName);
        byte GetNewBuffSlot(IBuff b);
        uint GetObjHash();
        bool HasBuff(IBuff buff);
        bool HasBuff(string buffName);
        void RemoveBuff(IBuff b);
        void RemoveBuff(string b);
        void RemoveBuffSlot(IBuff b);
        void RemoveBuffsWithName(string buffName);
        void RemoveCrowdControl(ICrowdControl cc);
        void RemoveStatModifier(IStatsModifier statModifier);
        void ResetAutoAttackSpellData();
        void SetDashingState(bool state);
        void SetTargetUnit(IAttackableUnit target);
        void StopMovement();
        void TeleportTo(float x, float y);
        void UpdateTargetUnit(IAttackableUnit unit);
    }
}
