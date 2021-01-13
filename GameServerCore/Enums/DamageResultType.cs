namespace GameServerCore.Enums
{
    public enum DamageResultType : byte
    {
        RESULT_INVULNERABLE = 0x0,
        RESULT_INVULNERABLENOMESSAGE = 0x1,
        RESULT_DODGE = 0x2,
        RESULT_CRITICAL = 0x3,
        RESULT_NORMAL = 0x4,
        RESULT_MISS = 0x5,
    }
}
