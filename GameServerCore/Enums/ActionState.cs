using System;

namespace GameServerCore.Enums
{
    [Flags]
    public enum ActionState : uint
    {
        CAN_ATTACK = 1 << 0,
        CAN_CAST = 1 << 1,
        CAN_MOVE = 1 << 2,
        CAN_NOT_MOVE = 1 << 3,
        STEALTHED = 1 << 4,
        REVEAL_SPECIFIC_UNIT = 1 << 5,
        TAUNTED = 1 << 6,
        FEARED = 1 << 7,
        IS_FLEEING = 1 << 8,
        CAN_NOT_ATTACK = 1 << 9,
        IS_ASLEEP = 1 << 10,
        IS_NEAR_SIGHTED = 1 << 11,
        IS_GHOSTED = 1 << 12,

        CHARMED = 1 << 15,
        NO_RENDER = 1 << 16,
        FORCE_RENDER_PARTICLES = 1 << 17,

        UNKNOWN = 1 << 23 // set to 1 by default, interferes with targetability
    }
}