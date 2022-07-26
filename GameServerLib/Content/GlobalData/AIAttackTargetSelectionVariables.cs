namespace LeagueSandbox.GameServer.Content
{
    public class AIAttackTargetSelectionVariables
    {
        /// <summary>
        /// Impact of each neighbor on the cost to selecting this target 
        /// </summary>
        public float TargetDistanceFactorPerNeightbor { get; set; } = 0.6f;
        /// <summary>
        /// Impact of each attacker on the cost to selecting this target
        /// </summary>
        public float TargetDistanceFactorPerAttacker { get; set; } = 0.8f;
        /// <summary>
        /// Impact o target strait line distance from source on cost estimate 
        /// </summary>
        public float TargetRangeFactor { get; set; } = 0.7f;
        /// <summary>
        /// Impact of actual path distance from source to target on final cost 
        /// </summary>
        public float TargetPathFactor { get; set; } = 0.5f;
        /// <summary>
        /// Additional distance added to distance between to discourage minons attacking heroes
        /// </summary>
        public float MinionTargetingHeroBoost { get; set; } = 150.0f;
        /// <summary>
        /// Maximun number of attackers already attacking target
        /// </summary>
        public int TargetMaxNumAttackers { get; set; } = 5;
    }
}
