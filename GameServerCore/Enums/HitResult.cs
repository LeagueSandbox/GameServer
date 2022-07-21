namespace GameServerCore.Enums
{
    public enum HitResult : byte
    {
        HIT_Normal = 0x0,
        HIT_Critical = 0x1,
        HIT_Dodge = 0x2,
        HIT_Miss = 0x3,
    }
}