using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using GameServerCore;
using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Maps;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.Logging;
using LeagueSandbox.GameServer.Scripting.CSharp;
using log4net;
using MapScripts;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;

namespace LeagueSandbox.GameServer.Maps
{
    /// <summary>
    /// Class responsible for all map related game settings such as collision handler, navigation grid, announcer events, and map properties.
    /// </summary>
    public class MapScriptHandler : IMapScriptHandler
    {
        // Crucial Vars
        protected Game _game;
        public CSharpScriptEngine _scriptEngine;
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
        /// Navigation Grid to be instanced by the map. Used for terrain data.
        /// </summary>
        public INavigationGrid NavigationGrid { get; private set; }
        public IMapData MapData { get; private set; }
        /// <summary>
        /// MapProperties specific to a Map Id. Contains information about passive gold gen, lane minion spawns, experience to level, etc.
        /// </summary>
        public IMapScript MapScript { get; private set; }

        //Investigate if we'd want All building to be handled directly in the MapScripts
        public Dictionary<LaneID, List<Vector2>> BlueMinionPathing;
        public Dictionary<LaneID, List<Vector2>> PurpleMinionPathing;
        public Dictionary<string, IMapObject> SpawnBarracks { get; set; }
        public List<INexus> NexusList { get; set; } = new List<INexus>();
        public List<IMapObject> InfoPoints { get; set; } = new List<IMapObject>();
        public Dictionary<TeamId, Dictionary<LaneID, List<ILaneTurret>>> TurretList { get; set; }
        public Dictionary<TeamId, Dictionary<LaneID, List<IInhibitor>>> InhibitorList { get; set; }
        public Dictionary<TeamId, IFountain> FountainList { get; set; } = new Dictionary<TeamId, IFountain>();
        public Dictionary<TeamId, IGameObject> ShopList { get; set; } = new Dictionary<TeamId, IGameObject>();
        public Dictionary<int, List<IMonster>> Monsters = new Dictionary<int, List<IMonster>>();
        public Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> PlayerSpawnPoints { get; set; } = new Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>>();

        public readonly Dictionary<TeamId, SurrenderHandler> Surrenders = new Dictionary<TeamId, SurrenderHandler>();

        /// <summary>
        /// Instantiates map related game settings such as collision handler, navigation grid, announcer events, and map properties.
        /// </summary>
        /// <param name="game">Game instance.</param>
        public MapScriptHandler(Game game)
        {
            _game = game;
            MapData = game.Config.MapData;
            _scriptEngine = game.ScriptEngine;
            _logger = LoggerProvider.GetLogger();
            Id = _game.Config.GameConfig.Map;

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

            MapScript = _scriptEngine.CreateObject<IMapScript>($"MapScripts.Map{Id}", $"{game.Config.GameConfig.GameMode}") ?? new EmptyMapScript();

            if (game.Config.MapData.SpawnBarracks != null)
            {
                SpawnBarracks = game.Config.MapData.SpawnBarracks;
            }

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
            MapScript.Update(diff);

            //TODO:Port everything bellow to MapScripts.
            if (MapScript.MapScriptMetadata.MinionSpawnEnabled)
            {
                if (_minionNumber > 0)
                {
                    // Spawn new Minion every 0.8s
                    if (_game.GameTime >= MapScript.NextSpawnTime + _minionNumber * 8 * 100)
                    {
                        if (SetUpLaneMinion())
                        {
                            _minionNumber = 0;
                            MapScript.NextSpawnTime = (long)_game.GameTime + MapScript.MapScriptMetadata.SpawnInterval;
                        }
                        else
                        {
                            _minionNumber++;
                        }
                    }
                }
                else if (_game.GameTime >= MapScript.NextSpawnTime)
                {
                    SetUpLaneMinion();
                    _minionNumber++;
                }
            }
            foreach (var surrender in Surrenders.Values)
            {
                surrender.Update(diff);
            }

            if (MapScript.MapScriptMetadata.EnableFountainHealing)
            {
                foreach (var fountain in FountainList.Values)
                {
                    fountain.Update(diff);
                }
            }
        }

        /// <summary>
        /// Initializes MapProperties. Usually only occurs once before players are added to Game.
        /// </summary>
        public void Init()
        {
            LoadMapInfo();
            MapScript.Init(this);
            if (MapScript.MapScriptMetadata.EnableBuildingProtection)
            {
                LoadBuildingProtection();
            }
            SpawnBuildings();
        }

        public void LoadMapInfo()
        {
            TurretList = new Dictionary<TeamId, Dictionary<LaneID, List<ILaneTurret>>>{
                { TeamId.TEAM_BLUE, new Dictionary<LaneID, List<ILaneTurret>>{ { LaneID.NONE, new List<ILaneTurret>() },{ LaneID.TOP, new List<ILaneTurret>()}, {LaneID.MIDDLE, new List<ILaneTurret>()}, {LaneID.BOTTOM, new List<ILaneTurret>()} } },
                { TeamId.TEAM_PURPLE, new Dictionary<LaneID, List<ILaneTurret>>{ { LaneID.NONE, new List<ILaneTurret>() }, { LaneID.TOP, new List<ILaneTurret>()}, {LaneID.MIDDLE, new List<ILaneTurret>()}, {LaneID.BOTTOM, new List<ILaneTurret>()} } },
                { TeamId.TEAM_NEUTRAL, new Dictionary<LaneID, List<ILaneTurret>>{ { LaneID.NONE, new List<ILaneTurret>() }, { LaneID.TOP, new List<ILaneTurret>()}, {LaneID.MIDDLE, new List<ILaneTurret>()}, {LaneID.BOTTOM, new List<ILaneTurret>()} }}};

            InhibitorList = new Dictionary<TeamId, Dictionary<LaneID, List<IInhibitor>>>{
                { TeamId.TEAM_BLUE, new Dictionary<LaneID, List<IInhibitor>>{{LaneID.TOP, new List<IInhibitor>()}, {LaneID.MIDDLE, new List<IInhibitor>()}, {LaneID.BOTTOM, new List<IInhibitor>()} } },
                { TeamId.TEAM_PURPLE, new Dictionary<LaneID, List<IInhibitor>>{{LaneID.TOP, new List<IInhibitor>()}, {LaneID.MIDDLE, new List<IInhibitor>()}, {LaneID.BOTTOM, new List<IInhibitor>()} } },
                { TeamId.TEAM_NEUTRAL, new Dictionary<LaneID, List<IInhibitor>>{{LaneID.TOP, new List<IInhibitor>()}, {LaneID.MIDDLE, new List<IInhibitor>()}, {LaneID.BOTTOM, new List<IInhibitor>()} }}};

            BlueMinionPathing = new Dictionary<LaneID, List<Vector2>> { { LaneID.NONE, new List<Vector2>() }, { LaneID.TOP, new List<Vector2>() }, { LaneID.MIDDLE, new List<Vector2>() }, { LaneID.BOTTOM, new List<Vector2>() } };
            PurpleMinionPathing = new Dictionary<LaneID, List<Vector2>> { { LaneID.NONE, new List<Vector2>() }, { LaneID.TOP, new List<Vector2>() }, { LaneID.MIDDLE, new List<Vector2>() }, { LaneID.BOTTOM, new List<Vector2>() } };

            //Investigate if we can unhardcode these variables
            var inhibRadius = 214;
            var nexusRadius = 353;
            var sightRange = 1700;
            List<IMapObject> missedTurrets = new List<IMapObject>();
            foreach (var mapObject in MapData.MapObjects.Values)
            {
                GameObjectTypes objectType = mapObject.GetGameObjectType();

                if (objectType == 0)
                {
                    continue;
                }
                TeamId teamId = mapObject.GetTeamID();
                LaneID lane = mapObject.GetLaneID();
                Vector2 position = new Vector2(mapObject.CentralPoint.X, mapObject.CentralPoint.Z);
                // Models are specific to team.
                string teamName = mapObject.GetTeamName();

                // Nexus
                if (objectType == GameObjectTypes.ObjAnimated_HQ && teamId != TeamId.TEAM_NEUTRAL)
                {
                    //Nexus model changes dont seem to take effect in-game
                    CreateNexus(mapObject.Name, MapScript.NexusModels[teamId], position, teamId, nexusRadius, sightRange);
                }
                // Inhibitors
                else if (objectType == GameObjectTypes.ObjAnimated_BarracksDampener)
                {
                    //Inhibitor model changes dont seem to take effect in-game
                    CreateInhibitor(mapObject.Name, MapScript.InhibitorModels[teamId], position, teamId, lane, inhibRadius, sightRange);
                }
                // Turrets
                else if (objectType == GameObjectTypes.ObjAIBase_Turret)
                {
                    if (mapObject.Name.Contains("Shrine"))
                    {

                        CreateLaneTurret(mapObject.Name + "_A", MapScript.TowerModels[teamId][TurretType.FOUNTAIN_TURRET], position, teamId, TurretType.FOUNTAIN_TURRET, LaneID.NONE, MapScript.LaneTurretAI, mapObject);
                        continue;
                    }

                    int index = mapObject.ParseIndex();

                    var turretType = MapScript.GetTurretType(index, lane, teamId);

                    if (turretType == TurretType.FOUNTAIN_TURRET)
                    {
                        missedTurrets.Add(mapObject);
                        continue;
                    }

                    CreateLaneTurret(mapObject.Name + "_A", MapScript.TowerModels[teamId][turretType], position, teamId, turretType, lane, MapScript.LaneTurretAI, mapObject);
                }
                else if (objectType == GameObjectTypes.InfoPoint)
                {
                    InfoPoints.Add(mapObject);
                }
                else if (objectType == GameObjectTypes.ObjBuilding_SpawnPoint)
                {
                    CreateFountain(teamId, position);
                }
                else if (objectType == GameObjectTypes.ObjBuilding_NavPoint)
                {
                    BlueMinionPathing[lane].Add(new Vector2(mapObject.CentralPoint.X, mapObject.CentralPoint.Z));
                }
                else if (objectType == GameObjectTypes.ObjBuilding_Shop)
                {
                    CreateShop(mapObject.Name, position, teamId);
                }
            }

            //If the map doesn't have any Minion pathing file but the map script has Minion pathing hardcoded
            if ((BlueMinionPathing.Count == 0 || MapScript.MapScriptMetadata.MinionPathingOverride) && MapScript.MinionPaths != null && MapScript.MinionPaths.Count != 0)
            {
                foreach (var lane in MapScript.MinionPaths.Keys)
                {
                    //Makes sure the coordinate list is empty
                    BlueMinionPathing[lane].Clear();
                    foreach (var value in MapScript.MinionPaths[lane])
                    {
                        BlueMinionPathing[lane].Add(value);
                    }
                }
            }

            //Sets purple team pathing by reversing Blue Team's pathing and adds an extra path coordinate towards the minions' spawn point.
            foreach (var lane in BlueMinionPathing.Keys)
            {
                foreach (var value in BlueMinionPathing[lane])
                {
                    PurpleMinionPathing[lane].Add(value);
                }
                PurpleMinionPathing[lane].Reverse();

                //The unhardcoded system results on minions stop walking right next to the nexus/nexus towers (since the last waypoint of a given minion, is the first one of the opsoite team, which isn't next to towers/nexus).
                //TODO: Decide if we want to hardcode extra waypoints in order to force the minion to walk towards the nexus or let it somehow be handled automatically by the minion's A.I
                var Barracks = SpawnBarracks.Values.ToList().FindAll(x => x.GetLaneID() == lane);
                foreach (var barrack in Barracks)
                {
                    if (barrack.GetTeamID() == TeamId.TEAM_PURPLE)
                    {
                        BlueMinionPathing[lane].Add(new Vector2(barrack.CentralPoint.X, barrack.CentralPoint.Z));
                    }
                    else
                    {
                        PurpleMinionPathing[lane].Add(new Vector2(barrack.CentralPoint.X, barrack.CentralPoint.Z));
                    }
                }
            }
        }
    }
}