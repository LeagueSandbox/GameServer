using GameServerCore.Domain;

namespace LeagueSandbox.GameServer.Content
{
    // TODO: Add Constants.var files to each Map's Content folder and assign values by reading them, currently this Data is only for Map1 as a placeholder.
    public class AIVars : IAIVars
    {
        /// <summary>
        /// Global AI enable/disable variable.
        /// </summary>
        public bool Enabled { get; set; } = true;
        /// <summary>
        /// Radius for experience sharing.
        /// </summary>
        public float EXPRadius { get; set; } = 1600.0f;
        /// <summary>
        /// Radius for gold sharing.
        /// </summary>
        public float GoldRadius { get; set; } = 1000.0f;
        /// <summary>
        /// Gold granted on game startup.
        /// </summary>
        public float StartingGold { get; set; } = 475.0f;
        /// <summary>
        /// Radius used for determining if a pet should return to its owner.
        /// </summary>
        public float DefaultPetReturnRadius { get; set; } = 200.0f;
        /// <summary>
        /// Time after game start where gold starts to generate.
        /// </summary>
        public float AmbientGoldDelay { get; set; } = 90.0f;
        /// <summary>
        /// Time after game start where gold starts to generate, assuming first blood has happened.
        /// </summary>
        public float AmbientGoldDelayFirstBlood { get; set; } = 30.0f;
    }
}
