using System;
namespace GameServerCore.Enums
{
    public enum ScoreCategory : byte
    {
        Offense = 0x0,
        Defense = 0x1,
        Combat = 0x2,
        Objective = 0x3,
        Max = 0x4,
        Total = 0x5,
    }
}
