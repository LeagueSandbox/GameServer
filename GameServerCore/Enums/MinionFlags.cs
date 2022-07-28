using System;

namespace GameServerCore.Enums
{
    [Flags]
    public enum MinionFlags
    {
        IsMinion = 0x1,
        IsTower = 0x2,
        AlwaysVisible = 0x4,
        AlwaysUpdatePAR = 0x8,
        IsJungleMonster = 0x10
    }
}
