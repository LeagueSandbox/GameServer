namespace GameServerCore.Domain
{
    public interface IAIVars
    {
        /// <summary>
        /// Global AI enable/disable variable.
        /// </summary>
        bool Enabled { get; set; }
        /// <summary>
        /// Radius for experience sharing.
        /// </summary>
        float EXPRadius { get; set; }
        /// <summary>
        /// Radius for gold sharing.
        /// </summary>
        float GoldRadius { get; set; }
        /// <summary>
        /// Gold granted on game startup.
        /// </summary>
        float StartingGold { get; set; }
        /// <summary>
        /// Radius used for determining if a pet should return to its owner.
        /// </summary>
        float DefaultPetReturnRadius { get; set; }
        /// <summary>
        /// Time after game start where gold starts to generate.
        /// </summary>
        float AmbientGoldDelay { get; set; }
        /// <summary>
        /// Time after game start where gold starts to generate, assuming first blood has happened.
        /// </summary>
        float AmbientGoldDelayFirstBlood { get; set; }
    }
}