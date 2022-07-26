namespace LeagueSandbox.GameServer.Content
{
    public class SpellVampVariables
    {
        /// <summary>
        /// Spell Vamp ratio for DAMAGESOURCE_SPELL
        /// </summary>
        public float SpellRatio { get; set; } = 1.0f;
        /// <summary>
        /// Spell Vamp ratio for DAMAGESOURCE_SPELLAOE
        /// </summary>
        public float SpellAoERatio { get; set; } = 0.334f;
        /// <summary>
        /// Spell Vamp ratio for DAMAGESOURCE_SPELLPERSIST
        /// </summary>
        public float SpellPersistRatio { get; set; } = 1.0f;
        /// <summary>
        /// Spell Vamp ratio for DAMAGESOURCE_PERIODIC
        /// </summary>
        public float PeriodicRatio { get; set; } = 0.0f;
        /// <summary>
        /// Spell Vamp ratio for DAMAGESOURCE_PROC
        /// </summary>
        public float ProcRatio { get; set; } = 0.0f;
        /// <summary>
        /// Spell Vamp ratio for DAMAGESOURCE_REACTIVE
        /// </summary>
        public float ReactiveRatio { get; set; } = 0.0f;
        /// <summary>
        /// Spell Vamp ratio for DAMAGESOURCE_ONDEATH
        /// </summary>
        public float OnDeathRatio { get; set; } = 0.0f;
        /// <summary>
        /// Spell Vamp ratio for DAMAGESOURCE_PET
        /// </summary>
        public float PetRatio { get; set; } = 0.0f;
    }
}
