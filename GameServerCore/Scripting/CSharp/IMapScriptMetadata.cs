using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;

namespace GameServerCore.Scripting.CSharp
{
    public interface IMapScriptMetadata
    {
        /// <summary>
        /// The base gold value of all champions in the match (Default: 300.0f)
        /// </summary>
        float ChampionBaseGoldValue { get; }
        /// <summary>
        /// The maximum gold value a champion can reach in the match (Default: 500.0f)
        /// </summary>
        float ChampionMaxGoldValue { get; }
        /// <summary>
        /// The Minimun gold value a champion can reach in the match (Default: 50.0f)
        /// </summary>
        float ChampionMinGoldValue { get; }
        /// <summary>
        /// Whether or not the fountain should heal the players (Default = true)
        /// </summary>
        bool EnableFountainHealing { get; }
        /// <summary>
        /// Wether or not building should have protection. Don't change this if you don't know what you're doing! (Default: false)
        /// </summary>
        bool EnableBuildingProtection { get; }
        /// <summary>
        /// The ammount of gold a player should be rewarded for getting the first blood (Default: 100.0f)
        /// </summary>
        float FirstBloodExtraGold { get; }
        /// <summary>
        /// Time when all players should start generating gold (Default: 90 Seconds)
        /// </summary>
        float FirstGoldTime { get; }
        /// <summary>
        /// The level of all players at the start of the game (Default: 1)
        /// </summary>
        int InitialLevel { get; }
        /// <summary>
        /// Whether or not players should be rewarded a decreasing ammount of gold based on how many times they died since their last kill (Default: true)
        /// </summary>
        bool IsKillGoldRewardReductionActive { get; }
        /// <summary>
        /// Ammount of gold per second for all players (Default: 1.9f)
        /// </summary>
        float GoldPerSecond { get; }
        /// <summary>
        /// Maximum level a player can reach (Default: 18)
        /// </summary>
        int MaxLevel { get; }
        /// <summary>
        /// Whether or not minions are enabled
        /// </summary>
        bool MinionSpawnEnabled { get; set; }
        /// <summary>
        /// Whether the map should use hardcoded minion path coordinates or use the one from league's files (if available) (Default: false)
        /// </summary>
        bool MinionPathingOverride { get; }
        /// <summary>
        /// Wether or not the map's position is to be overriden
        /// </summary>
        bool OverrideSpawnPoints { get; }
        /// <summary>
        /// The ItemID that should be used as the "Recall Spell", default recall spell is "BluePill", with ID: 2001 (Default: 2001)
        /// </summary>
        int RecallSpellItemId { get; }
        /// <summary>
        /// Time between minion waves (Default: 30 seconds)
        /// </summary>
        long SpawnInterval { get; set; }
        /// <summary>
        /// Initial ammount of gold for all players (Default 475.0f)
        /// </summary>
        float StartingGold { get; }
    }
}
