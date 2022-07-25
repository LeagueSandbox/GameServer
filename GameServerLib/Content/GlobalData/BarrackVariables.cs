namespace LeagueSandbox.GameServer.Content
{
    public class BarrackVariables
    {
        /// <summary>
        /// Enable/disable spawn from barracks
        /// </summary>
        public bool BSpawnEnabled { get; set; } = true;
        /// <summary>
        /// armor
        /// </summary>
        public int Armor { get; set; } = 0;
        /// <summary>
        /// max hp
        /// </summary>
        public int MaxHP { get; set; } = 4000;
        /// <summary>
        /// max hp for tutorial mode
        /// </summary>
        public int MaxHPTutorial { get; set; } = 7000;
    }
}
