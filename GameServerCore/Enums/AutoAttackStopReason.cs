namespace GameServerCore.Enums
{
    public enum AutoAttackStopReason : int
    {
        TargetLost = 0x0,
        Moving = 0x1,
        AttackDelayed = 0x2,
        OtherImmediately = 0x3,
        OtherFinishAnim = 0x4,
    }
}
