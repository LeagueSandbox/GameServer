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
    }
}
