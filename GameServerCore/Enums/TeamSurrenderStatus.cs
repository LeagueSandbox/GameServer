namespace GameServerCore.Enums
{
    public enum TeamSurrenderStatus
    {
        VoetWasNoSurrender = 0x0,
        NotAllowedYet = 0x1,
        DontSpamSurrender = 0x2,
        AlreadyVoted = 0x3,
        SurrenderAgreed = 0x4,
        EarlySurrenderAllowed = 0x5,
    }
}
