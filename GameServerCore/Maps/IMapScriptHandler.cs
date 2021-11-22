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
        IMinion CreateMinion(string name, string model, Vector2 position, uint netId = 0, TeamId team = TeamId.TEAM_NEUTRAL, int skinId = 0, bool ignoreCollision = false, bool isTargetable = false);
        void AddObject(IGameObject obj);
        IRegion CreateRegion(TeamId team, Vector2 position, RegionType type = RegionType.Default, IGameObject collisionUnit = null, IGameObject visionTarget = null, bool giveVision = false, float visionRadius = 0, bool revealStealth = false, bool hasCollision = false, float collisionRadius = 0, float grassRadius = 0, float scale = 1, float addedSize = 0, float lifeTime = 0, int clientID = 0);
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
        void AddLevelProp(string name, string model, Vector2 position, float height, Vector3 direction, Vector3 posOffset, Vector3 scale, int skinId = 0, byte skillLevel = 0, byte rank = 0, byte type = 2, uint netId = 0, byte netNodeId = 64);
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
        void LoadBuildingProtection();
    }
}