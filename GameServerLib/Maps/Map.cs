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
using GameServerCore.Domain.GameObjects;
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
using MapScripts;

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
        /// <summary>
        /// MapProperties specific to a Map Id. Contains information about passive gold gen, lane minion spawns, experience to level, etc.
        /// </summary>
        public IMapScript MapScript { get; private set; }
        /// <summary>
        /// List of events related to the announcer (ex: first blood)
        /// </summary>
        public List<IAnnounce> AnnouncerEvents { get; private set; }

        public List<INexus> _nexus { get; set; } = new List<INexus>();
        public Dictionary<TeamId, Dictionary<LaneID, List<IInhibitor>>> _inhibitors { get; set; } = new Dictionary<TeamId, Dictionary<LaneID, List<IInhibitor>>> { { TeamId.TEAM_BLUE, new Dictionary<LaneID, List<IInhibitor>>() }, { TeamId.TEAM_PURPLE, new Dictionary<LaneID, List<IInhibitor>>() } };
        public Dictionary<TeamId, Dictionary<LaneID, List<ILaneTurret>>> _turrets { get; set; } = new Dictionary<TeamId, Dictionary<LaneID, List<ILaneTurret>>> { { TeamId.TEAM_BLUE, new Dictionary<LaneID, List<ILaneTurret>>() }, { TeamId.TEAM_PURPLE, new Dictionary<LaneID, List<ILaneTurret>>() } };
        public Dictionary<TeamId, IFountain> _fountains { get; set; } = new Dictionary<TeamId, IFountain>();
        private readonly Dictionary<TeamId, SurrenderHandler> _surrenders = new Dictionary<TeamId, SurrenderHandler>();

        /// <summary>
        /// Instantiates map related game settings such as collision handler, navigation grid, announcer events, and map properties.
        /// </summary>
        /// <param name="game">Game instance.</param>
        public Map(Game game)
        {
            _game = game;
            _mapData = game.Config.MapData;
            _scriptEngine = game.ScriptEngine;
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
            MapScript = GetMapProperties(Id);
            //scriptEngine.CreateObject<IMapScript>("MapScripts", $"Map{Id}") ?? new MapScriptEmpty();
            //MapScript.StartUp(_game);
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

            MapScript.Update(diff);
        }

        /// <summary>
        /// Initializes MapProperties. Usually only occurs once before players are added to Game.
        /// </summary>
        public void Init()
        {
            LoadBuildings();
            MapScript.Init(this);
            LoadBuildingProtection();
        }

        public IMapScript GetMapProperties(int mapId)
        {
            var dict = new Dictionary<int, Type>
            {
                // [0] = typeof(FlatTestMap),
                [1] = typeof(Map1),
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
                return new Map1();
            }
            return (IMapScript)Activator.CreateInstance(dict[mapId]);
        }

        public void LoadBuildings()
        {
            foreach (TeamId team in _inhibitors.Keys)
            {
                if (MapScript.HasTopLane)
                {
                    _turrets[team].Add(LaneID.TOP, new List<ILaneTurret>());

                    _inhibitors[team].Add(LaneID.TOP, new List<IInhibitor>());
                }
                if (MapScript.HasMidLane)
                {
                    _turrets[team].Add(LaneID.MIDDLE, new List<ILaneTurret>());

                    _inhibitors[team].Add(LaneID.MIDDLE, new List<IInhibitor>());
                }
                if (MapScript.HasBotLane)
                {
                    _turrets[team].Add(LaneID.BOTTOM, new List<ILaneTurret>());

                    _inhibitors[team].Add(LaneID.BOTTOM, new List<IInhibitor>());
                }
            }

            // Below is where we create the buildings.

            // These two are used for fixing any wrongly indexed turrets that are present.
            var inhibRadius = 214;
            var nexusRadius = 353;
            var sightRange = 1700;
            Dictionary<TeamId, List<IMapObject>> test = new Dictionary<TeamId, List<IMapObject>> { { TeamId.TEAM_BLUE, new List<IMapObject>() }, { TeamId.TEAM_PURPLE, new List<IMapObject>() } };
            List<IMapObject> missedTurrets = new List<IMapObject>();

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
                        _turrets[teamId][LaneID.MIDDLE].Add(new LaneTurret(_game, mapObject.Name + "_A", teamName + GetTowerModel(TurretType.FOUNTAIN_TURRET, teamId), position, teamId, TurretType.FOUNTAIN_TURRET, GetTurretItems(TurretType.FOUNTAIN_TURRET), 0, LaneID.MIDDLE, mapObject));
                        continue;
                    }

                    int index = mapObject.ParseIndex();

                    // Failed to find an index in the turret's name, skip it altogether since it would be invalid.
                    if (index == -1)
                    {
                        // TODO: Verify if we should still add them; they would be assigned to lane NONE as a fountain turret.
                        continue;
                    }

                    var turretType = MapScript.GetTurretType(index, lane);

                    if (turretType == TurretType.FOUNTAIN_TURRET)
                    {
                        missedTurrets.Add(mapObject);
                        continue;
                    }

                    // index - 1 as we need it to start at 0.
                    _turrets[teamId][lane].Add(new LaneTurret(_game, mapObject.Name + "_A", teamName + GetTowerModel(turretType, teamId), position, teamId, turretType, GetTurretItems(turretType), 0, lane, mapObject));
                }
                else if (objectType == GameObjectTypes.ObjBuilding_SpawnPoint)
                {
                    AddFountain(teamId, position);
                }
            }
        }
        public void LoadBuildingProtection()
        {
            var teamInhibitors = new Dictionary<TeamId, List<IInhibitor>>
            {
                { TeamId.TEAM_BLUE, new List<IInhibitor>() },
                { TeamId.TEAM_PURPLE, new List<IInhibitor>() }
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
            List<IInhibitor> allInhibitors = new List<IInhibitor>();
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

        //Bellow is stuff to comunicate info between this script and the map script

        //Tower stuff
        public int[] GetTurretItems(TurretType type)
        {
            if (!MapScript.TurretItems.ContainsKey(type))
            {
                return null;
            }

            return MapScript.TurretItems[type];
        }
        public void ChangeTowerOnMapList(string towerName, TeamId team, LaneID currentLaneId, LaneID desiredLaneID)
        {
            var tower = _turrets[team][currentLaneId].Find(x => x.Name == towerName);
            tower.SetLaneID(desiredLaneID);
            _turrets[team][currentLaneId].Remove(tower);
            _turrets[team][desiredLaneID].Add(tower);
        }
        public string GetTowerModel(TurretType type, TeamId teamId)
        {
            return MapScript.TowerModels[teamId][type];
        }

        //Inhibitor stuff
        public IInhibitor GetInhibitorById(uint id)
        {
            foreach (TeamId team in _inhibitors.Keys)
            {
                foreach (LaneID lane in _inhibitors[team].Keys)
                {
                    foreach (var inhibitor in _inhibitors[team][lane])
                    {
                        if (inhibitor.NetId == id)
                        {
                            return inhibitor;
                        }
                    }
                }
            }
            return null;
        }
        public bool AllInhibitorsDestroyedFromTeam(TeamId team)
        {
            foreach (LaneID lane in _inhibitors[team].Keys)
            {
                foreach (var inhibitor in _inhibitors[team][lane])
                {
                    if (inhibitor.Team == team && inhibitor.InhibitorState == InhibitorState.ALIVE)
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        //Minion Stuff
        public Tuple<TeamId, Vector2> GetMinionSpawnPosition(string spawnPosition)
        {
            var coords = _mapData.SpawnBarracks[spawnPosition].CentralPoint;

            var teamID = TeamId.TEAM_BLUE;
            if (spawnPosition.Contains("Chaos"))
            {
                teamID = TeamId.TEAM_PURPLE;
            }
            return new Tuple<TeamId, Vector2>(teamID, new Vector2(coords.X, coords.Z));
        }
        public void SpawnMinion(List<MinionSpawnType> list, int minionNo, string barracksName, List<Vector2> waypoints)
        {
            if (list.Count <= minionNo)
            {
                return;
            }

            var team = GetMinionSpawnPosition(barracksName).Item1;
            var m = new LaneMinion(_game, list[minionNo], barracksName, waypoints, MapScript.MinionModels[team][list[minionNo]], 0, team);
            _game.ObjectManager.AddObject(m);
        }
        public Dictionary<string, IMapObject> GetSpawnBarracks()
        {
            return _mapData.SpawnBarracks;
        }
        public bool IsMinionSpawnEnabled()
        {
            return _game.Config.MinionSpawnsEnabled;
        }


        //General Map stuff, such as Announcements and surrender
        //TODO: See if the "IsMapSpecific" parameter is actually needed.
        public void AddAnnouncement(long time, Announces ID, bool IsMapSpecific)
        {
            AnnouncerEvents.Add(new Announce(_game, time, ID, IsMapSpecific));
        }
        public void AddObject(Vector2 position, float height, Vector3 direction, float unk1, float unk2, string name, string model, byte skin = 0, uint NetId = 0)
        {
            _game.ObjectManager.AddObject(new LevelProp(_game, position, height, direction, unk1, unk2, name, model, skin, NetId));
        }
        public void AddSurrender(float time, float restTime, float length)
        {
            _surrenders.Add(TeamId.TEAM_BLUE, new SurrenderHandler(_game, TeamId.TEAM_BLUE, time, restTime, length));
            _surrenders.Add(TeamId.TEAM_PURPLE, new SurrenderHandler(_game, TeamId.TEAM_PURPLE, time, restTime, length));
        }
        public void AddFountain(TeamId team, Vector2 position)
        {
            _fountains.Add(team, new Fountain(_game, team, position, 1000));
        }

        //Game Time
        public float GameTime()
        {
            return _game.GameTime;
        }

    }
}
