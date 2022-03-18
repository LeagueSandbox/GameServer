using System;
namespace GameServerCore.Enums
{
    public enum QuestEvent : byte
    {
        Press = 0x0,
        Release = 0x1,
        Rollover = 0x2,
        Rollout = 0x3,
    }
}
