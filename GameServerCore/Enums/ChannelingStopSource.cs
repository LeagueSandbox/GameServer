namespace GameServerCore.Enums
{
    // TODO: Verify if this contains all possible values.
    public enum ChannelingStopSource
    {
        NotCancelled,
        TimeCompleted,
        /// <summary>
        /// Unknown
        /// </summary>
        Animation,
        LostTarget,
        StunnedOrSilencedOrTaunted,
        ChannelingCondition,
        Die,
        HeroReincarnate,
        Move,
        Attack,
        Casting,
        Unknown
    }
}
