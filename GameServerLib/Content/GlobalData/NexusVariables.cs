namespace LeagueSandbox.GameServer.Content
{
    public class NexusVariables
    {
        /// <summary>
        /// Time it takes to pan to the Nexus during the EoG ceremony
        /// </summary>
        public float EoGPanTime { get; set; } = 3.0f;
        /// <summary>
        /// Time before the explosion effect is played on Nexus during the EoG ceremony
        /// </summary>
        public float EoGNexusExplosionTime { get; set; } = 3.5f;
        /// <summary>
        /// Flag to tell the Nexus to use its death animation at EoG
        /// </summary>
        public bool EoGUseNexusDeathAnimation { get; set; } = true;
        /// <summary>
        /// Time before the explosion effect and the skin swap is played on Nexus during the EoG ceremony
        /// </summary>
        public float EoGNexusChangeSkinTime { get; set; } = 3.5f;
    }
}
