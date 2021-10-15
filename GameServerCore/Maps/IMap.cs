using System.Collections.Generic;
using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;


namespace GameServerCore.Maps
{
    /// <summary>
    /// Contains all map related game settings such as collision handler, navigation grid, announcer events, and map properties.
    /// </summary>
    public interface IMap : IUpdate
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
        /// <summary>
        /// MapProperties specific to a Map Id. Contains information about passive gold gen, lane minion spawns, experience to level, etc.
        /// </summary>
        IMapProperties MapProperties { get; }
        /// <summary>
        /// List of events related to the announcer (ex: first blood)
        /// </summary>
        List<IAnnounce> AnnouncerEvents { get; }
        /// <summary>
        /// List of all nexus in-game
        /// </summary>
        List<INexus> _nexus { get; }
        /// <summary>
        /// List of all turrets in-game
        /// </summary>
        Dictionary<TeamId, Dictionary<LaneID, List<ILaneTurret>>> _turrets { get; }
        /// <summary>
        /// List of all inhibitors in-game
        /// </summary>
        Dictionary<TeamId, Dictionary<LaneID, List<IInhibitor>>> _inhibitors { get; }
        /// <summary>
        /// Initializes MapProperties. Usually only occurs once before players are added to Game.
        /// </summary>
        void Init();
    }
}
