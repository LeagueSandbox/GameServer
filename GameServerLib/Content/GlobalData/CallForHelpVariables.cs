namespace LeagueSandbox.GameServer.Content
{
    //Default value used here came from Map12, as Map1 doesn't include them (probably because it uses default values)
    //TODO: Figure out Map1's default values
    public class CallForHelpVariables
    {
        /// <summary>
        /// How often a unit will issue a Call For Help
        /// </summary>
        public float Delay { get; set; } = 1.0f;
        /// <summary>
        /// How long a unit should ignore lower prioty calls while the curent target is not activly attacking
        /// </summary>
        public float Stick { get; set; } = 1.5f;
        /// <summary>
        /// Units within this radius will hear your Call For Help
        /// </summary>
        public float Radius { get; set; } = 800.0f;
        /// <summary>
        /// How long a unit will consider a Call For Help.  Mainly used to track whether a unit has already responded.
        /// </summary>
        public float Duration { get; set; } = 1.0f;
        /// <summary>
        /// Attack range buffer distance for melee responders to a Call For Help
        /// </summary>
        public float MeleeRadius { get; set; } = 420.0f;
        /// <summary>
        /// Attack range buffer distance for ranged responders to a Call For Help
        /// </summary>
        public float RangedRadius { get; set; } = 170.0f;
        /// <summary>
        /// Attack range buffer distance for turret responders to a Call For Help
        /// </summary>
        public float TurretRadius { get; set; } = 1.0f;
    }
}
