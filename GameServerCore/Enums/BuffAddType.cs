namespace GameServerCore.Enums
{
    /// <summary>
    /// Determines how a buff should be treated when added.
    /// </summary>
    public enum BuffAddType
    {
        /// <summary>
        /// Replaces any existing buffs of the same name.
        /// </summary>
        REPLACE_EXISTING,
        /// <summary>
        /// Restarts the timer on any buffs of the same name already applied to the buff's target.
        /// </summary>
        RENEW_EXISTING,
        /// <summary>
        /// Adds a stack to any buffs of the same name already applied and restarts all other stacks' timers.
        /// Functionally treated as a single buff with a single timer and stack count.
        /// </summary>
        STACKS_AND_RENEWS,
        /// <summary>
        /// Adds a stack to any buffs of the same name already applied and continues the timer of the oldest stack, while postponing the timer of newer stacks until the oldest has finished.
        /// Functionally treated as a single buff with a single timer and stack count.
        /// </summary>
        STACKS_AND_CONTINUE,
        /// <summary>
        /// Adds a completely new buff instance to the buff target regardless of any other buffs of the same name applied.
        /// Inherits stack count of the oldest buff of the same name.
        /// </summary>
        STACKS_AND_OVERLAPS
    }
}