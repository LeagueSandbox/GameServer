namespace LeagueSandbox.GameServer.Content
{
    public class ObjAIBaseVariables
    {
        /// <summary>
        /// Enable/disable AI
        /// </summary>
        public bool AIToggle { get; set; } = true;
        /// <summary>
        /// Should AI pathing ignore buildings and not take them into account in their lane positioning?
        /// </summary>
        public bool PathIgnoresBuildings { get; set; } = false;
        /// <summary>
        /// Radius for Experience sharing
        /// </summary>
        public float ExpRadius2 = 1600.0f;
        /// <summary>
        /// Radius for Gold sharing
        /// </summary>
        public float GoldRadius2 = 1000.0f;
        /// <summary>
        /// Gold granted on startup
        /// </summary>
        public float StartingGold = 475.0f;
        /// <summary>
        /// Default pet return radius
        /// </summary>
        public float DefaultPetReturnRadius = 200.0f;
        /// <summary>
        /// Delay before first ambient gold tick
        /// </summary>
        public float AmbientGoldDelay = 90.0f;
        /// <summary>
        /// Delay before first ambient gold tick on First Blood
        /// </summary>
        public float AmbientGoldDelayFirstBlood = 30.0f;
    }
}
