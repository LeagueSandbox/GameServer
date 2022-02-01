using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;

namespace GameServerCore.Scripting.CSharp
{
    public interface IMapScriptMetadata
    {
        bool EnableBuildingProtection { get; set; }
        /// <summary>
        /// Whether or not minions are enabled (This can cause issues if enabled and all structures aren't perfectly setup)
        /// </summary>
        bool MinionSpawnEnabled { get; set; }
        /// <summary>
        /// Time between minion waves (Default: 30 seconds)
        /// </summary>
        long SpawnInterval { get; set; }
        /// <summary>
        /// Whether the map should use hardcoded minion path coordinates or use the one from league's files (if available) (Default: false)
        /// </summary>
        bool MinionPathingOverride { get; set; }
        /// <summary>
        /// Ammount of gold per second for all players (Default: 1.9f)
        /// </summary>
        float GoldPerSecond { get; set; }
        /// <summary>
        /// Initial ammount of gold for all players (Default 475.0f)
        /// </summary>
        float StartingGold { get; set; }
        /// <summary>
        /// Whether or not players should be rewarded a decreasing ammount of gold based on how many times they died since their last kill (Default: true)
        /// </summary>
        bool IsKillGoldRewardReductionActive { get; set; }
        /// <summary>
        /// The ItemID that should be used as the "Recall Spell", default recall spell is "BluePill", with ID: 2001 (Default: 2001)
        /// </summary>
        int RecallSpellItemId { get; set; }
        /// <summary>
        /// Time when all players should start generating gold (Default: 90 Seconds)
        /// </summary>
        float FirstGoldTime { get; set; }
        /// <summary>
        /// Whether or not the fountain should heal the players (Default = true)
        /// </summary>
        bool EnableFountainHealing { get; set; }
        /// <summary>
        /// Wether or not the map's position is to be overriden
        /// </summary>
        bool OverrideSpawnPoints { get; set; }
        /// <summary>
        /// The level of all players at the start of the game
        /// </summary>
        int InitialLevel { get; set; }
        /// <summary>
        /// Maximum level a player can reach
        /// </summary>
        int MaxLevel { get; set; }
    }
}
