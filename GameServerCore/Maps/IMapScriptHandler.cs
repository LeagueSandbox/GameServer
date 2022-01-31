using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeaguePackets.Game.Common;

namespace GameServerCore.Maps
{
    /// <summary>
    /// Contains all map related game settings such as collision handler, navigation grid, announcer events, and map properties. Doubles as a Handler/Manager for all MapScripts.
    /// </summary>
    public interface IMapScriptHandler : IUpdate
    {
        /// <summary>
        /// Unique identifier for the Map (ex: 1 = Old SR, 11 = New SR)
        /// </summary>
        int Id { get; }
        /// <summary>
        /// Collision Handler to be instanced by the map. Used for collisions between GameObjects or GameObjects and terrain.
        /// </summary>
        ICollisionHandler CollisionHandler { get; }
        /// <summary>
        /// Navigation Grid to be instanced by the map. Used for terrain data.
        /// </summary>
        INavigationGrid NavigationGrid { get; }
        IMapData MapData { get; }
        /// <summary>
        /// MapScript which determines functionality for a specific map. Contains information about passive gold gen, lane minion spawns, experience to level, etc.
        /// </summary>
        IMapScript MapScript { get; }
        /// <summary>
        /// List of events related to the announcer (ex: first blood)
        /// </summary>
        List<IAnnounce> AnnouncerEvents { get; }
        /// <summary>
        /// List of all nexus in-game
        /// </summary>
        List<INexus> NexusList { get; }
        /// <summary>
        /// List of all turrets in-game
        /// </summary>
        Dictionary<TeamId, Dictionary<LaneID, List<ILaneTurret>>> TurretList { get; }
        /// <summary>
        /// List of all capture points in-game
        /// </summary>
        List<IMapObject> InfoPoints { get; }
        /// <summary>
        /// List of all inhibitors in-game
        /// </summary>
        Dictionary<TeamId, Dictionary<LaneID, List<IInhibitor>>> InhibitorList { get; }
        /// <summary>
        /// List of fountains
        /// </summary>
        Dictionary<TeamId, IFountain> FountainList { get; }
        /// <summary>
        /// List of map Shops
        /// </summary>
        Dictionary<TeamId, IGameObject> ShopList { get; set; }
        /// <summary>
        /// Coordinates for player SpawnPositions when the match first begins
        /// </summary>
        Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> PlayerSpawnPoints { get; set; }
        /// <summary>
        /// List of LaneMinion's spawn points
        /// </summary>
        Dictionary<string, IMapObject> SpawnBarracks { get; }
        /// <summary>
        /// Initializes MapProperties. Usually only occurs once before players are added to Game.
        /// </summary>
        void Init();
        /// <summary>
        /// Changes the lane of a tower in the _tower list, in order to fix missplaced towers due to riot's seemingly random naming scheme
        /// </summary>
        /// <param name="towerName"></param>
        /// <param name="team"></param>
        /// <param name="currentLaneId"></param>
        /// <param name="desiredLaneID"></param>
        void ChangeTowerOnMapList(string towerName, TeamId team, LaneID currentLaneId, LaneID desiredLaneID);
        /// <summary>
        /// Spawns a turret Manually
        /// </summary>
        /// <param name="turret"></param>
        /// <param name="protectionDependsOfAll"></param>
        /// <param name="protectedBy"></param>
        void SpawnTurret(ILaneTurret turret, bool hasProtection, bool protectionDependsOfAll = false, IAttackableUnit[] protectedBy = null);
        /// <summary>
        /// Loads the Buildings protection system (Note: This can break a lot of maps if turrets types and lanes arent properly setup)
        /// </summary>
        void LoadBuildingProtection();
        IInhibitor GetInhibitorById(uint id);
        bool AllInhibitorsDestroyedFromTeam(TeamId team);
        bool IsMinionSpawnEnabled();
        Tuple<TeamId, Vector2> GetMinionSpawnPosition(string spawnPosition);
        /// <summary>
        /// Spawns a LaneMinion
        /// </summary>
        /// <param name="list"></param>
        /// <param name="minionNo"></param>
        /// <param name="barracksName"></param>
        /// <param name="waypoints"></param>
        void CreateLaneMinion(List<MinionSpawnType> list, int minionNo, string barracksName, List<Vector2> waypoints);
        IMinion CreateMinion
        (
            string name,
            string model,
            Vector2 position,
            uint netId = 0,
            TeamId team = TeamId.TEAM_NEUTRAL,
            int skinId = 0,
            bool ignoreCollision = false,
            bool isTargetable = false,
            string aiScript = "",
            int damageBonus = 0,
            int healthBonus = 0,
            int initialLevel = 1
        );
        /// <summary>
        /// Adds an annoucement
        /// </summary>
        /// <param name="time"></param>
        /// <param name="ID"></param>
        void AddAnnouncement(long time, EventID ID, bool isMapSpecific);
        /// <summary>
        /// Adds a prop to the map
        /// </summary>
        /// <param name="position"></param>
        /// <param name="height"></param>
        /// <param name="direction"></param>
        /// <param name="unk1"></param>
        /// <param name="unk2"></param>
        /// <param name="name"></param>
        /// <param name="model"></param>
        /// <param name="skin"></param>
        /// <param name="NetId"></param>
        ILevelProp AddLevelProp(string name, string model, Vector2 position, float height, Vector3 direction, Vector3 posOffset, Vector3 scale, int skinId = 0, byte skillLevel = 0, byte rank = 0, byte type = 2, uint netId = 0, byte netNodeId = 64);
        void NotifyPropAnimation(ILevelProp prop, string animation, AnimationFlags animationFlag, float duration, bool destroyPropAfterAnimation);
        /// <summary>
        /// Sets up the surrender functionality
        /// </summary>
        /// <param name="time"></param>
        /// <param name="restTime"></param>
        /// <param name="length"></param>
        void AddSurrender(float time, float restTime, float length);
        void HandleSurrender(int userId, IChampion who, bool vote);
        IMonsterCamp CreateJungleCamp(Vector3 position, byte groupNumber, TeamId teamSideOfTheMap, string campTypeIcon, float respawnTimer, bool doPlayVO = false, byte revealEvent = 74, float spawnDuration = 0.0f);
        void CreateJungleMonster
        (
            string name, string model, Vector2 position, Vector3 faceDirection,
            IMonsterCamp monsterCamp, TeamId team = TeamId.TEAM_NEUTRAL, string spawnAnimation = "", uint netId = 0,
            bool isTargetable = true, bool ignoresCollision = false, string aiScript = "",
            int damageBonus = 0, int healthBonus = 0, int initialLevel = 1
        );
        void SpawnCamp(IMonsterCamp monsterCamp);
        void SetMinimapIcon(IAttackableUnit unit, string iconCategory = "", bool changeIcon = false, string borderCategory = "", bool changeBorder = false);
        /// <summary>
        /// Returns how long the match has been going on for.
        /// </summary>
        /// <returns></returns>
        float GameTime();
        /// <summary>
        /// Sets the features which should be enabled for this map. EX: Mana, Cooldowns, Lane Minions, etc. Refer to FeatureFlags enum.
        /// </summary>
        /// <returns></returns>
        void SetGameFeatures(FeatureFlags featureFlag, bool isEnabled);
        /// <summary>
        /// Sets the game to exit
        /// </summary>
        /// <param name="losingTeam">The team who lost the game</param>
        /// <param name="finalCameraPosition">The position which the camera has to move to for the end-game screen</param>
        /// <param name="endGameTimer">Offset for the Endgame screend (victory or defeat) to be actually announced</param>
        /// <param name="moveCamera">Wether or not the camera should move</param>
        /// <param name="cameraTimer">The ammount of time the camera has to arrive to it's destination</param>
        /// <param name="disableUI">Whether or not the UI should get disabled</param>
        /// <param name="deathData">DeathData of what triggered the End of the Game, such as necus death</param>
        void EndGame(TeamId losingTeam, Vector3 finalCameraPosition, float endGameTimer = 5000.0f, bool moveCamera = true, float cameraTimer = 3.0f, bool disableUI = true, IDeathData deathData = null);
    }
}
