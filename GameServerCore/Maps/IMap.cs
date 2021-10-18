﻿using System;
using System.Collections.Generic;
using System.Numerics;
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
        IMapScript MapScript { get; }
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
        /// List of fountains
        /// </summary>
        Dictionary<TeamId, IFountain> _fountains { get; }
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
        void SpawnMinion(List<MinionSpawnType> list, int minionNo, string barracksName, List<Vector2> waypoints);
        /// <summary>
        /// Adds an annoucement
        /// </summary>
        /// <param name="time"></param>
        /// <param name="ID"></param>
        /// <param name="IsMapSpecific"></param>
        void AddAnnouncement(long time, Announces ID, bool IsMapSpecific);
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
        void AddObject(Vector2 position, float height, Vector3 direction, float unk1, float unk2, string name, string model, byte skin = 0, uint NetId = 0);
        /// <summary>
        /// Sets up the surrender functionality
        /// </summary>
        /// <param name="time"></param>
        /// <param name="restTime"></param>
        /// <param name="length"></param>
        void AddSurrender(float time, float restTime, float length);
        void HandleSurrender(int userId, IChampion who, bool vote);
        /// <summary>
        /// Returns how long the match has been going on for.
        /// </summary>
        /// <returns></returns>
        float GameTime();
    }
}
