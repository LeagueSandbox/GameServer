namespace LeagueSandbox.GameServer.Content
{
    public class ObjAIBuildingVariables
    {
        /// <summary>
        /// Amount of time for a hero attack to be counted towards a building kill credit
        /// </summary>
        public float TimerForBuildingKillCredit { get; set; } = 30.0f;
        /// <summary>
        /// Amount of time to pass before sending another damage building event for the announcer
        /// </summary>
        public float TimerBeforeSendingDamageEvent { get; set; } = 22.0f;
        /// <summary>
        /// Minimum amount of health % (0->1.0) a building must have to send a damage event
        /// </summary>
        public float MinimumHealthForDamageEvent { get; set; } = 0.250f;
        /// <summary>
        /// Minimum number of minions that have to be near the tower for the building to send a damage event. Will ignore if there is at least one champion in range
        /// </summary>
        public float MinimumNumberOfMinionsForDamageEvent { get; set; } = 10.0f;
        /// <summary>
        /// Radius that is considered in-range of the building
        /// </summary>
        public float DamageEventRadius { get; set; } = 2000.0f;
        /// <summary>
        /// How long a tower must be under attack to send the damage event
        /// </summary>
        public float ConstantAttackTimeForDamageEvent { get; set; } = 3.0f;
        /// <summary>
        /// Time that passes for a structure no long considered under attack
        /// </summary>
        public float NoDamageCancelTime { get; set; } = 1.25f;
    }
}
