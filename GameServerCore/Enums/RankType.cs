using System;
namespace GameServerCore.Enums
{
    public enum RankType : uint
    {
        SOLDIER = 0x0,
        LIEUTENANT = 0x1,
        CAPTAIN = 0x2,
        GENERAL = 0x3,
        NUMTYPE_0 = 0x4,
        UNKNOWN_0 = 0xFFFFFFFE,
    }
}
