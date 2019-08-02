namespace GameServerCore.Packets.Enums
{
    public enum SurrenderReason : uint
    {
        SURRENDER_FAILED = 0,
        SURRENDER_TOO_EARLY = 1,
        SURRENDER_TOO_QUICKLY = 2,
        SURRENDER_ALREADY_VOTED = 3,
        SURRENDER_PASSED = 4
    }
}