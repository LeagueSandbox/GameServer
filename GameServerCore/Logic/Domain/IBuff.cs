using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GameServerCore.Logic.Domain.GameObjects;
using GameServerCore.Logic.Enums;

namespace GameServerCore.Logic.Domain
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
