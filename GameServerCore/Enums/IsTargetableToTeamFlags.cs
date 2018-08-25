using System;

namespace GameServerCore.Enums
{
    [Flags]
    public enum IsTargetableToTeamFlags : uint
    {
        NON_TARGETABLE_ALLY = 0x800000,
        NON_TARGETABLE_ENEMY = 0x1000000,
        TARGETABLE_TO_ALL = 0x2000000
    }
}