namespace LeagueSandbox.GameServer.Content
{
    public class SpawnPointVariables
    {
        /// <summary>
        /// The spawn point will heal units in this radius.
        /// </summary>
        public float RegenRadius { get; set; } = 1100.0f;
        /// <summary>
        /// The percentage of max HP a unit will regenerate per tick.  A float value between 0.0 and 1.0.
        /// </summary>
        public float HealthRegenPercent { get; set; } = 0.085f;
        /// <summary>
        /// Same as above, for ARAM. Needed because we use the same map for ARAM as we do the tutorial
        /// </summary>
        public float HealthRegenPercentARAM { get; set; } = 0.0f;
        /// <summary>
        /// The percentage of max Mana a unit will regenerate per tick.  A float value between 0.0 and 1.0.
        /// </summary>
        public float ManaRegenPercent { get; set; } = 0.085f;
        /// <summary>
        /// Interval between spawn point regen ticks.
        /// </summary>
        public float RegenTickInterval { get; set; } = 1.0f;
    }
}
