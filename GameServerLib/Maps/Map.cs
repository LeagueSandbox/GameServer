using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Force.Crc32;
using GameServerCore;
using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Enums;
using GameServerCore.Maps;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.Logging;
using LeagueSandbox.GameServer.Scripting.CSharp;
using log4net;

namespace LeagueSandbox.GameServer.Maps
{
    /// <summary>
    /// Class responsible for all map related game settings such as collision handler, navigation grid, announcer events, and map properties.
    /// </summary>
    public class Map : IMap
    {
        // Crucial Vars
        protected Game _game;
        public MapData _mapData;
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
        /// <summary>
        /// MapProperties specific to a Map Id. Contains information about passive gold gen, lane minion spawns, experience to level, etc.
        /// </summary>
        public IMapProperties MapProperties { get; private set; }
        /// <summary>
        /// List of events related to the announcer (ex: first blood)
        /// </summary>
        public List<IAnnounce> AnnouncerEvents { get; private set; }

        private Dictionary<TeamId, Fountain> _fountains = new Dictionary<TeamId, Fountain>();
        private List<Nexus> _nexus;
        private Dictionary<TeamId, Dictionary<LaneID, List<Inhibitor>>> _inhibitors;
        private Dictionary<TeamId, Dictionary<LaneID, List<LaneTurret>>> _turrets;


        /// <summary>
        /// Instantiates map related game settings such as collision handler, navigation grid, announcer events, and map properties.
        /// </summary>
        /// <param name="game">Game instance.</param>
        public Map(Game game)
        {
            _game = game;
            _mapData = game.Config.MapData;
            //_loadMapStructures = _game.Config.LoadMapStructures;
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

            AnnouncerEvents = new List<IAnnounce>();
            CollisionHandler = new CollisionHandler(this);
            MapProperties = GetMapProperties(Id);
        }

        /// <summary>
        /// Function called every tick of the game. Updates CollisionHandler, MapProperties, and executes AnnouncerEvents.
        /// </summary>
        /// <param name="diff">Number of milliseconds since this tick occurred.</param>
        public void Update(float diff)
        {
            CollisionHandler.Update();
            foreach (var announce in AnnouncerEvents)
            {
                if (!announce.IsAnnounced && _game.GameTime >= announce.EventTime)
                {
                    announce.Execute();
                }
            }

            MapProperties.Update(diff);
        }

        /// <summary>
        /// Initializes MapProperties. Usually only occurs once before players are added to Game.
        /// </summary>
        public void Init()
        {
            LoadBuildings();
            MapProperties.Init();
            LoadBuildingProtection();
        }

        public IMapProperties GetMapProperties(int mapId)
        {
            var dict = new Dictionary<int, Type>
            {
                // [0] = typeof(FlatTestMap),
                [1] = typeof(SummonersRift),
                // [2] = typeof(HarrowingRift),
                // [3] = typeof(ProvingGrounds),
                // [4] = typeof(TwistedTreeline),
                // [6] = typeof(WinterRift),
                // [8] = typeof(CrystalScar),
                // [10] = typeof(NewTwistedTreeline),
                // [11] = typeof(NewSummonersRift),
                //[12] = typeof(HowlingAbyss)
                // [14] = typeof(ButchersBridge)
            };

            if (!dict.ContainsKey(mapId))
            {
                return new SummonersRift(_game);
            }

            return (IMapProperties)Activator.CreateInstance(dict[mapId], _game);
        }

        public void LoadBuildings()
        {
            _nexus = new List<Nexus>();
            Dictionary<LaneID, List<LaneTurret>> turretLanesToAdd = new Dictionary<LaneID, List<LaneTurret>>();
            Dictionary<LaneID, List<Inhibitor>> inhibitorLanedToAdd = new Dictionary<LaneID, List<Inhibitor>>();
            if (MapProperties.HasTopLane)
            {
                turretLanesToAdd.Add(LaneID.TOP, new List<LaneTurret>());

                inhibitorLanedToAdd.Add(LaneID.TOP, new List<Inhibitor>());
            }
            if (MapProperties.HasMidLane)
            {
                turretLanesToAdd.Add(LaneID.MIDDLE, new List<LaneTurret>());

                inhibitorLanedToAdd.Add(LaneID.MIDDLE, new List<Inhibitor>());
            }
            if (MapProperties.HasBotLane)
            {
                turretLanesToAdd.Add(LaneID.BOTTOM, new List<LaneTurret>());

                inhibitorLanedToAdd.Add(LaneID.BOTTOM, new List<Inhibitor>());
            }
            _inhibitors = new Dictionary<TeamId, Dictionary<LaneID, List<Inhibitor>>> { { TeamId.TEAM_BLUE, inhibitorLanedToAdd }, { TeamId.TEAM_PURPLE, inhibitorLanedToAdd } };
            _turrets = new Dictionary<TeamId, Dictionary<LaneID, List<LaneTurret>>> { { TeamId.TEAM_BLUE, turretLanesToAdd }, { TeamId.TEAM_PURPLE, turretLanesToAdd } };

            // Below is where we create the buildings.

            // These two are used for fixing any wrongly indexed turrets that are present.
            var inhibRadius = 214;
            var nexusRadius = 353;
            var sightRange = 1700;
            Dictionary<TeamId, List<MapData.MapObject>> test = new Dictionary<TeamId, List<MapData.MapObject>> { { TeamId.TEAM_BLUE, new List<MapData.MapObject>() }, { TeamId.TEAM_PURPLE, new List<MapData.MapObject>() } };
            List<MapData.MapObject> missedTurrets = new List<MapData.MapObject>();

            foreach (var mapObject in _mapData.MapObjects.Values)
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
                if (objectType == GameObjectTypes.ObjAnimated_HQ)
                {
                    _nexus.Add(new Nexus(_game, teamName + "Nexus", teamId, nexusRadius, position, sightRange, Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(mapObject.Name)) | 0xFF000000));
                }
                // Inhibitors
                else if (objectType == GameObjectTypes.ObjAnimated_BarracksDampener)
                {
                    _inhibitors[teamId][lane].Add(new Inhibitor(_game, teamName + "Inhibitor", lane, teamId, inhibRadius, position, sightRange, Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(mapObject.Name)) | 0xFF000000));
                    test[teamId].Add(mapObject);
                }
                // Turrets
                else if (objectType == GameObjectTypes.ObjAIBase_Turret)
                {
                    if (mapObject.Name.Contains("Shrine"))
                    {
                        _turrets[teamId][LaneID.MIDDLE].Add(new LaneTurret(_game, mapObject.Name + "_A", teamName + MapProperties.GetTowerModel(TurretType.FOUNTAIN_TURRET, teamId), position, teamId, TurretType.FOUNTAIN_TURRET, MapProperties.GetTurretItems(TurretType.FOUNTAIN_TURRET), 0, LaneID.MIDDLE, mapObject));
                        continue;
                    }

                    int index = mapObject.ParseIndex();

                    // Failed to find an index in the turret's name, skip it altogether since it would be invalid.
                    if (index == -1)
                    {
                        // TODO: Verify if we should still add them; they would be assigned to lane NONE as a fountain turret.
                        continue;
                    }

                    var turretType = _mapData.GetTurretType(index, lane);

                    if (turretType == TurretType.FOUNTAIN_TURRET)
                    {
                        missedTurrets.Add(mapObject);
                        continue;
                    }

                    // index - 1 as we need it to start at 0.
                    _turrets[teamId][lane].Add(new LaneTurret(_game, mapObject.Name + "_A", teamName + MapProperties.GetTowerModel(turretType, teamId), position, teamId, turretType, MapProperties.GetTurretItems(turretType), 0, lane, mapObject));
                }
                else if (objectType == GameObjectTypes.ObjBuilding_SpawnPoint)
                {
                    _fountains.Add(teamId, new Fountain(_game, teamId, position, 1000));
                }
            }
        }
        public void LoadBuildingProtection()
        {
            var teamInhibitors = new Dictionary<TeamId, List<Inhibitor>>
            {
                { TeamId.TEAM_BLUE, new List<Inhibitor>() },
                { TeamId.TEAM_PURPLE, new List<Inhibitor>() }
            };

            var teams = teamInhibitors.Keys.ToList();
            foreach (var team in teams)
            {
                _inhibitors[team].Values.ToList().ForEach(l => teamInhibitors[team].AddRange(l));
            }

            foreach (var nexus in _nexus)
            {
                // Adds Protection to Nexus
                _game.ProtectionManager.AddProtection
                (
                    nexus,
                    _turrets[nexus.Team][LaneID.MIDDLE].FindAll(turret => turret.Type == TurretType.NEXUS_TURRET).ToArray(),
                    teamInhibitors[nexus.Team].ToArray()
                );

                // Adds Nexus
                _game.ObjectManager.AddObject(nexus);
            }

            // Iterate through all inhibitors for both teams.
            List<Inhibitor> allInhibitors = new List<Inhibitor>();
            allInhibitors.AddRange(teamInhibitors[TeamId.TEAM_BLUE]);
            allInhibitors.AddRange(teamInhibitors[TeamId.TEAM_PURPLE]);

            foreach (var inhibitor in allInhibitors)
            {
                var inhibitorTurret = _turrets[inhibitor.Team][inhibitor.Lane].First(turret => turret.Type == TurretType.INHIBITOR_TURRET);

                // Adds Protection to Inhibitors
                if (inhibitorTurret != null)
                {
                    // Depends on the first available inhibitor turret.
                    _game.ProtectionManager.AddProtection(inhibitor, false, inhibitorTurret);
                }

                // Adds Inhibitors
                _game.ObjectManager.AddObject(inhibitor);

                // Adds Protection to Turrets
                foreach (var turret in _turrets[inhibitor.Team][inhibitor.Lane])
                {
                    if (turret.Type == TurretType.NEXUS_TURRET)
                    {
                        _game.ProtectionManager.AddProtection(turret, false, _inhibitors[inhibitor.Team][inhibitor.Lane].ToArray());
                    }
                    else if (turret.Type == TurretType.INHIBITOR_TURRET)
                    {
                        _game.ProtectionManager.AddProtection(turret, false, _turrets[inhibitor.Team][inhibitor.Lane].First(dependTurret => dependTurret.Type == TurretType.INNER_TURRET));
                    }
                    else if (turret.Type == TurretType.INNER_TURRET)
                    {
                        _game.ProtectionManager.AddProtection(turret, false, _turrets[inhibitor.Team][inhibitor.Lane].First(dependTurret => dependTurret.Type == TurretType.OUTER_TURRET));
                    }

                    // Adds Turrets
                    _game.ObjectManager.AddObject(turret);
                }
            }
        }
    }
}
