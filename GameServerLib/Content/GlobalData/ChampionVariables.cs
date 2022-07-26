namespace LeagueSandbox.GameServer.Content
{
    public class ChampionVariables
    {
        /// <summary>
        /// Gold granted on ambient gold tick
        /// </summary>
        public float AmbientGoldAmount { get; set; } = 9.5f;
        /// <summary>
        /// Interval between ambient gold ticks
        /// </summary>
        public float AmbientGoldInterval { get; set; } = 5.0f;
        /// <summary>
        /// Disable ambient gold while dead
        /// </summary>
        public bool DisableAmbientGoldWhileDead { get; set; } = false;
        /// <summary>
        /// Delay before first ambient XP tick
        /// </summary>
        public float AmbientXPDelay { get; set; } = 0.0f;
        /// <summary>
        /// Interval between ambient XP ticks
        /// </summary>
        public float AmbientXPInterval { get; set; } = 5.0f;
        /// <summary>
        /// XP granted on ambient XP tick
        /// </summary>
        public float AmbientXPAmount { get; set; } = 0.0f;
        /// <summary>
        /// XP granted on ambient XP tick in tutorial mode
        /// </summary>
        public float AmbientXPAmountTutorial { get; set; } = 0.0f;
        /// <summary>
        /// Disable ambient XP while dead
        /// </summary>
        public bool DisableAmbientXPWhileDead { get; set; } = false;
        /// <summary>
        /// Gold lost per level when you die
        /// </summary>
        public float GoldLostPerLevel { get; set; } = 0.0f;
        /// <summary>
        /// Time in seconds spent dead per level
        /// </summary>
        public float TimeDeadPerLevel { get; set; } = 4.0f;
        /// <summary>
        /// Coefficient for gold handicapping
        /// </summary>
        public float GoldHandicapCoefficient { get; set; } = 0.0f;
        /// <summary>
        /// Amount of time for something to be considered a multikill (double kill etc...)
        /// </summary>
        public float TimeForMultiKill { get; set; } = 30.0f;
        /// <summary>
        /// Amount of time for something to be considered a multikill (double kill etc...) while the target is the last man standing
        /// </summary>
        public float TimeForLastMultiKill { get; set; } = 10.0f;
        /// <summary>
        /// Amount of time for something to be considered an assist
        /// </summary>
        public float TimerForAssist { get; set; } = 10.0f;
        /// <summary>
        /// Minion denial percentage
        /// </summary>
        public float MinionDenialPercentage { get; set; } = 0.0f;
    }
}
