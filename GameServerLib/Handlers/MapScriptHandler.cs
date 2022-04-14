using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameServerCore;
using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Handlers;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.Logging;
using LeagueSandbox.GameServer.Scripting.CSharp;
using log4net;
using MapScripts;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;

namespace LeagueSandbox.GameServer.Handlers
{
    /// <summary>
    /// Class responsible for all map related game settings such as collision handler, navigation grid, announcer events, and map properties.
    /// </summary>
    public class MapScriptHandler : IMapScriptHandler
    {
        // Crucial Vars
        protected Game _game;
        public MapData _loadMapStructures;
        private readonly ILog _logger;

        /// <summary>
        /// Unique identifier for the Map (ex: 1 = Old SR, 11 = New SR)
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// Collision Handler to be instanced by the map. Used for collisions between GameObjects or GameObjects and terrain.
        /// </summary>
        public ICollisionHandler CollisionHandler { get; private set; }
        /// <summary>
        /// Pathing Handler to be instanced by the map. Used for pathfinding for units.
        /// </summary>
        public IPathingHandler PathingHandler { get; private set; }
        /// <summary>
        /// Navigation Grid to be instanced by the map. Used for terrain data.
        /// </summary>
        public INavigationGrid NavigationGrid { get; private set; }
        public IMapData MapData { get; private set; }
        /// <summary>
        /// MapProperties specific to a Map Id. Contains information about passive gold gen, lane minion spawns, experience to level, etc.
        /// </summary>
        public IMapScript MapScript { get; private set; }
        public Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> PlayerSpawnPoints { get; set; } = new Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>>();

        public readonly Dictionary<TeamId, SurrenderHandler> Surrenders = new Dictionary<TeamId, SurrenderHandler>();

        /// <summary>
        /// Instantiates map related game settings such as collision handler, navigation grid, announcer events, and map properties.
        /// </summary>
        /// <param name="game">Game instance.</param>
        public MapScriptHandler(Game game)
        {
            _game = game;
            _logger = LoggerProvider.GetLogger();
            Id = _game.Config.GameConfig.Map;

            MapScript = CSharpScriptEngine.CreateObjectStatic<IMapScript>($"MapScripts.Map{Id}", $"{game.Config.GameConfig.GameMode}") ?? new EmptyMapScript();

            if (MapScript.PlayerSpawnPoints != null && MapScript.MapScriptMetadata.OverrideSpawnPoints)
            {
                PlayerSpawnPoints = MapScript.PlayerSpawnPoints;
            }
            else
            {
                PlayerSpawnPoints = _game.Config.GetMapSpawns();
            }

            SetMap(game, this);
        }

        /// <summary>
        /// Function called every tick of the game. Updates CollisionHandler, MapProperties, and executes AnnouncerEvents.
        /// </summary>
        /// <param name="diff">Number of milliseconds since this tick occurred.</param>
        public void Update(float diff)
        {
            CollisionHandler.Update();
            PathingHandler.Update(diff);
            MapScript.Update(diff);

            foreach (var surrender in Surrenders.Values)
            {
                surrender.Update(diff);
            }

        }

        /// <summary>
        /// Initializes MapProperties. Usually only occurs once before players are added to Game.
        /// </summary>
        public void Init()
        {
            MapData = _game.Config.ContentManager.GetMapData(Id);

            try
            {
                NavigationGrid = _game.Config.ContentManager.GetNavigationGrid(Id);
            }
            catch (ContentNotFoundException exception)
            {
                _logger.Error(exception.Message);
                return;
            }

            CollisionHandler = new CollisionHandler(this);
            PathingHandler = new PathingHandler(this);

            Dictionary<GameObjectTypes, List<MapObject>> mapObjects = new Dictionary<GameObjectTypes, List<MapObject>>();
            foreach (var mapObject in MapData.MapObjects.Values)
            {
                GameObjectTypes objectType = mapObject.GetGameObjectType();

                if (objectType == 0)
                {
                    continue;
                }

                if (!mapObjects.ContainsKey(objectType))
                {
                    mapObjects.Add(objectType, new List<MapObject>());
                }

                mapObjects[objectType].Add(mapObject);
            }

            if (MapData != null)
            {
                mapObjects.Add(GameObjectTypes.ObjBuildingBarracks, new List<MapObject>());

                foreach (var spawnBarrack in MapData.SpawnBarracks)
                {
                    mapObjects[GameObjectTypes.ObjBuildingBarracks].Add(spawnBarrack.Value);
                }
            }

            MapScript.Init(mapObjects);
        }
    }
}