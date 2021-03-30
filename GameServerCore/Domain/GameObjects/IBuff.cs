using GameServerCore.Domain.GameObjects.Spell;
using GameServerCore.Enums;

namespace GameServerCore.Domain.GameObjects
{
    public interface IBuff : IStackable, IUpdate
    {
        BuffAddType BuffAddType { get; }
        BuffType BuffType { get; }
        float Duration { get; }
        bool IsHidden { get; }
        string Name { get; }
        ISpell OriginSpell { get; }
        byte Slot { get; }
        IObjAiBase SourceUnit { get; }
        IAttackableUnit TargetUnit { get; }
        float TimeElapsed { get; }

        void ActivateBuff();
        void DeactivateBuff();
        bool Elapsed();
        IStatsModifier GetStatsModifier();
        bool IsBuffInfinite();
        bool IsBuffSame(string buffName);
        void ResetTimeElapsed();
        void SetSlot(byte slot);
    }
}
