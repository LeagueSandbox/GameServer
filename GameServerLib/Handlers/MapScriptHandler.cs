using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.Logging;
using LeagueSandbox.GameServer.Scripting.CSharp;
using log4net;
using MapScripts;
using static GameServerCore.Content.HashFunctions;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Content.Navigation;
using GameServerCore.Domain;

namespace LeagueSandbox.GameServer.Handlers
{
    /// <summary>
    /// Class responsible for all map related game settings such as collision handler, navigation grid, announcer events, and map properties.
    /// </summary>
    public class MapScriptHandler
    {
        // Crucial Vars
        protected Game _game;
        public MapData _loadMapStructures;
        private static ILog _logger = LoggerProvider.GetLogger();

        /// <summary>
        /// Unique identifier for the Map (ex: 1 = Old SR, 11 = New SR)
        /// </summary>
        public int Id { get; private set; }
        /// <summary>
        /// Collision Handler to be instanced by the map. Used for collisions between GameObjects or GameObjects and terrain.
        /// </summary>
        public CollisionHandler CollisionHandler { get; private set; }
        /// <summary>
        /// Pathing Handler to be instanced by the map. Used for pathfinding for units.
        /// </summary>
        public PathingHandler PathingHandler { get; private set; }
        /// <summary>
        /// Navigation Grid to be instanced by the map. Used for terrain data.
        /// </summary>
        public NavigationGrid NavigationGrid { get; private set; }
        public MapData MapData { get; private set; }
        /// <summary>
        /// MapProperties specific to a Map Id. Contains information about passive gold gen, lane minion spawns, experience to level, etc.
        /// </summary>
        public IMapScript MapScript { get; private set; }
        public uint ScriptNameHash { get; private set; }
        public IEventSource ParentScript => null;

        public Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> PlayerSpawnPoints { get; set; } = new Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>>();

        //Consider moving this to MapScripts(?)
        public readonly Dictionary<TeamId, SurrenderHandler> Surrenders = new Dictionary<TeamId, SurrenderHandler>();

        /// <summary>
        /// Instantiates map related game settings such as collision handler, navigation grid, announcer events, and map properties.
        /// </summary>
        /// <param name="game">Game instance.</param>
        public MapScriptHandler(Game game)
        {
            _game = game;
            Id = _game.Config.GameConfig.Map;

            string scriptName = game.Config.GameConfig.GameMode;
            MapScript = CSharpScriptEngine.CreateObjectStatic<IMapScript>($"MapScripts.Map{Id}", scriptName) ?? new EmptyMapScript();
            ScriptNameHash = HashString(scriptName);

            if (MapScript.PlayerSpawnPoints != null && MapScript.MapScriptMetadata.OverrideSpawnPoints)
            {
                PlayerSpawnPoints = MapScript.PlayerSpawnPoints;
            }
            else
            {
                PlayerSpawnPoints = _game.Config.GetMapSpawns();
            }

            try
            {
                NavigationGrid = _game.Config.ContentManager.GetNavigationGrid(this);
            }
            catch (ContentNotFoundException exception)
            {
                _logger.Error(exception.Message);
                return;
            }

            CollisionHandler = new CollisionHandler(this);
            PathingHandler = new PathingHandler(this);
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
            GlobalData.Init(MapData.MapConstants);
            // Load data package
            try
            {
                MapScript.Init(LoadMapObjects());
            }
            catch (Exception e)
            {
                _logger.Error(null, e);
            }
        }

        Dictionary<GameObjectTypes, List<MapObject>> LoadMapObjects()
        {
            Dictionary<GameObjectTypes, List<MapObject>> toReturn = new Dictionary<GameObjectTypes, List<MapObject>>();
            foreach (var mapObject in MapData.MapObjects.Values)
            {
                GameObjectTypes objectType = mapObject.GetGameObjectType();

                if (objectType == 0)
                {
                    continue;
                }

                if (!toReturn.ContainsKey(objectType))
                {
                    toReturn.Add(objectType, new List<MapObject>());
                }

                toReturn[objectType].Add(mapObject);
            }

            toReturn.Add(GameObjectTypes.ObjBuildingBarracks, new List<MapObject>());

            foreach (var spawnBarrack in MapData.SpawnBarracks)
            {
                toReturn[GameObjectTypes.ObjBuildingBarracks].Add(spawnBarrack.Value);
            }

            return toReturn;
        }
    }
}