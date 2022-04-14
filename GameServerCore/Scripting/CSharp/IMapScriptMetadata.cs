using GameServerCore.Domain.GameObjects.Spell.Missile;
using GameServerCore.Domain.GameObjects.Spell.Sector;

namespace GameServerCore.Scripting.CSharp
{
    public interface IMapScriptMetadata
    {
        /// <summary>
        /// Ammount of gold all players receive every gold tick (Default: 0.95f)
        /// </summary>
        float BaseGoldPerGoldTick { get; }
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
        /// Used to override the default ExpCurve (Default: string.Empty)
        /// </summary>
        string ExpCurveOverride { get; }
        /// <summary>
        /// Max range where you can receive XP when a minion dies (Default: 1400.0f)
        /// </summary>
        float ExpRange { get; }
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
        /// How Fast gold should be added for players in milliseconds (Default: 500.0f);
        /// </summary>
        float GoldTickSpeed { get;}
        /// <summary>
        /// Functionality still unknown, I thought it was used in ARAM, but it's map script sets it to 0 (Default: 1250.0f)
        /// </summary>
        float GoldRange { get; }
        /// <summary>
        /// Maximum level a player can reach (Default: 18)
        /// </summary>
        int MaxLevel { get; }
        /// <summary>
        /// Whether or not minions are enabled
        /// </summary>
        bool MinionSpawnEnabled { get; set; }
        /// <summary>
        /// An override for the navGrid of an specific map, allowing gamemodes with custom navGrids (Ascension) to function
        /// </summary>
        string NavGridOverride { get; set; }
        /// <summary>
        /// Wether or not the map's position is to be overriden (Default: string.Empty)
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
