namespace GameServerCore.Enums
{
    public enum TeamType : uint
    {
        ALL = 0xFFFFFFFE,
        NONE = 0xFFFFFFFF,
        ORDER = 0x0,
        CHAOS = 0x1,
        NEUTRAL = 0x2,
        SPECIFIC = 0x3,
    }
}
