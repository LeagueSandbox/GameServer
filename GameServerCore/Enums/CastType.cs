namespace GameServerCore.Enums
{
    public enum CastType : int
    {
        CAST_Instant = 0x0,
        CAST_TargetMissile = 0x1,
        CAST_ChainMissile = 0x2,
        CAST_CircleMissile = 0x3,
        CAST_ArcMissile = 0x4,
        CAST_FORCEDWORD = 0x7FFFFFFF
    }
}