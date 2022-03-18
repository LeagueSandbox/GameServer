using System;
namespace GameServerCore.Enums
{
    public enum MusicCommand : byte
    {
        PrepareCue = 0x0,
        BeginCue = 0x1,
        EndCue = 0x2,
        StateChange = 0x3,
        TriggerEvent = 0x4,
    }
}
