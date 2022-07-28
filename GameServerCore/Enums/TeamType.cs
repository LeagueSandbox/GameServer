namespace GameServerCore.Enums
{
    //TeamType is different from TeamID, this is to be used on EventSourceInfo when it gets eventually introduced
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
