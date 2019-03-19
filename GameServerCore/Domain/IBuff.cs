using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface IBuff: IStackable, IUpdate
    {
        float Duration { get; }
        float TimeElapsed { get; }
        IObjAiBase TargetUnit { get; }
        IObjAiBase SourceUnit { get; }
        BuffType BuffType { get; }
        string Name { get; }
        byte Slot { get; }
        void ResetDuration();
        bool Elapsed();
    }
}
