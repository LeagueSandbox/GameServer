namespace LeagueSandbox.GameServer.Content
{
    public class AttackRangeVariables
    {
        public float ClosingAttackRangeModifier { get; set; } = 300.0f;
        public float StopAttackRangeModifier { get; set; } = 100.0f;
        /// <summary>
        /// The acquisition range for charmed units.
        /// </summary>
        public float AICharmedAcquisitionRange { get; set; } = 1000.0f;
    }
}
