namespace GameServerCore.Enums
{
    /// <summary>
    /// Source types for damage. Used in determining when damage is applied, such as before mitigation.
    /// </summary>
    public enum DamageSource
    {
        /// <summary>
        /// Unmitigated.
        /// </summary>
        DAMAGE_SOURCE_RAW,
        /// <summary>
        /// Executes, pure.
        /// </summary>
        DAMAGE_SOURCE_INTERNALRAW,
        /// <summary>
        /// Buff spell dots.
        /// </summary>
        DAMAGE_SOURCE_PERIODIC,
        /// <summary>
        /// Causes Proc (spell specific or attack based) events to fire, pre initial damage.
        /// </summary>
        DAMAGE_SOURCE_PROC,
        /// <summary>
        /// On proc.
        /// </summary>
        DAMAGE_SOURCE_REACTIVE,
        /// <summary>
        /// Unknown, self-explanatory?
        /// </summary>
        DAMAGE_SOURCE_ONDEATH,
        /// <summary>
        /// Single instance spell damage.
        /// </summary>
        DAMAGE_SOURCE_SPELL,
        /// <summary>
        /// Attack based spells (proc onhit effects).
        /// </summary>
        DAMAGE_SOURCE_ATTACK,
        /// <summary>
        /// Buff Summoner spell damage (single and multi instance)
        /// </summary>
        DAMAGE_SOURCE_DEFAULT,
        /// <summary>
        /// Any area based spells.
        /// </summary>
        DAMAGE_SOURCE_SPELLAOE,
        /// <summary>
        /// Passive, on update or timed repeat.
        /// </summary>
        DAMAGE_SOURCE_SPELLPERSIST,
        /// <summary>
        /// Unknown, self-explanatory?
        /// </summary>
        DAMAGE_SOURCE_PET
    }

}
