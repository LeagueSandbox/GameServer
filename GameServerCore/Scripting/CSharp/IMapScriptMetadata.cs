using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;

namespace GameServerCore.Scripting.CSharp
{
    public interface IMapScriptMetadata
    {
        bool EnableBuildingProtection { get; set; }
        /// <summary>
        /// Wether or not minions are enabled (This can cause issues if enabled and all structures aren't perfectly setup)
        /// </summary>
        bool MinionSpawnEnabled { get; set; }
        /// <summary>
        /// Time between minion waves (Default: 30 seconds)
        /// </summary>
        long SpawnInterval { get; set; }
        /// <summary>
        /// Wether the map should use hardcoded minion path coordinates or use the one from league's files (if available) (Default: false)
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
        /// Wether or not players should be rewarded a decreasing ammount of gold based on how many times they died since their last kill (Default: true)
        /// </summary>
        bool IsKillGoldRewardReductionActive { get; set; }
        /// <summary>
        /// What Item should be used as the "Recall Spell", default recall spell is "bluepill", with ID: 2001 (Deafult: 2001)
        /// </summary>
        int RecallSpellItemId { get; set; }
        /// <summary>
        /// Time when all players should start generating gold (Default: 90 Seconds)
        /// </summary>
        long FirstGoldTime { get; set; }
        /// <summary>
        /// Wether or not the fountain should heal the players (Default = true)
        /// </summary>
        bool EnableFountainHealing { get; set; }
    }
}
