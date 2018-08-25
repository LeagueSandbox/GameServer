using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;

namespace GameServerCore.Domain
{
    public interface IBuff
    {
        float Duration { get; }
        float TimeElapsed { get; }
        IObjAiBase TargetUnit { get; }
        IObjAiBase SourceUnit { get; }
        BuffType BuffType { get; }
        string Name { get; }
        int Stacks { get; }
        byte Slot { get; }

    }
}
