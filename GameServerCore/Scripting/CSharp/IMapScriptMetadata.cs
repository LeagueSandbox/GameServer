using GameServerCore.Domain;
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
        /// The ammount of gold a player should be rewarded for getting the first blood (Default: 100.0f)
        /// </summary>
        float FirstBloodExtraGold { get; }
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
        /// Interface of variables related to AI units on the map, such as EXP share range or pet return radius.
        /// </summary>
        IAIVars AIVars { get; set; }
    }
}
