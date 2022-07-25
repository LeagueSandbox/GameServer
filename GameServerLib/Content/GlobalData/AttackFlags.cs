namespace LeagueSandbox.GameServer.Content
{
    public class AttackFlags
    {
        /// <summary>
        /// The range of reveal for attackers
        /// </summary>
        public float RevealAttackerRange { get; set; } = 400.0f;
        /// <summary>
        /// The timeout for the reveal for attackers
        /// </summary>
        public float RevealAttackerTimeOut { get; set; } = 4.5f;
        /// <summary>
        /// Whats the minimum speed we will rotate to face a target during a spell cast?
        /// </summary>
        public float MinCastRotationSpeed { get; set; } = 250.0f;
        /// <summary>
        /// Height of the location circular location casting reticle
        /// </summary>
        public float TargetingReticleHeight { get; set; } = 40.0f;
    }
}
