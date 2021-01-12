namespace GameServerCore.Enums
{
    //int32?
    public enum GroundAttackMode : byte
    {
        Disabled = 0x0,
        ReplaceMove = 0x1,
        ReplaceSelect = 0x2,
        ReplaceMoveAndSelect = 0x3,
        ReplaceAttackMove = 0x4,
        AutoTriggerAtCursor = 0x5,
    }
}
