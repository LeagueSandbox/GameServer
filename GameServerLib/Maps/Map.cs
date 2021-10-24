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

        private int _minionNumber;
        private int _cannonMinionCount;
        public List<INexus> _nexus { get; set; } = new List<INexus>();
        public Dictionary<TeamId, IFountain> _fountains { get; set; } = new Dictionary<TeamId, IFountain>();
        private readonly Dictionary<TeamId, SurrenderHandler> _surrenders = new Dictionary<TeamId, SurrenderHandler>();
        public Dictionary<TeamId, Dictionary<LaneID, List<IInhibitor>>> _inhibitors { get; set; }
        public Dictionary<TeamId, Dictionary<LaneID, List<ILaneTurret>>> _turrets { get; set; }


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

            //Since there are quite a few Summoners Rift map variations (Map1, Map2, Map6, Map7), if no map script is found, it will default to a vanilla summoners rift script
            MapScript = _scriptEngine.CreateObject<IMapScript>("MapScripts", $"Map{Id}") ?? new SummonersRiftDefault();
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

            if (_game.GameTime >= 120 * 1000)
            {
                MapScript.IsKillGoldRewardReductionActive = false;
            }

            if (MapScript.SpawnEnabled)
            {
                if (_minionNumber > 0)
                {
                    if (_game.GameTime >= MapScript.NextSpawnTime + _minionNumber * 8 * 100)
                    { // Spawn new wave every 0.8s
                        if (Spawn())
                        {
                            _minionNumber = 0;
                            MapScript.NextSpawnTime = (long)_game.GameTime + MapScript.SpawnInterval;
                        }
                        else
                        {
                            _minionNumber++;
                        }
                    }
                }
                else if (_game.GameTime >= MapScript.NextSpawnTime)
                {
                    Spawn();
                    _minionNumber++;
                }
            }
            foreach (var surrender in _surrenders.Values)
            {
                surrender.Update(diff);
            }

            if (_fountains != null)
            {
                foreach (var fountain in _fountains.Values)
                {
                    fountain.Update(diff);
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
        }

        public void LoadBuildings()
        {
            _turrets = new Dictionary<TeamId, Dictionary<LaneID, List<ILaneTurret>>>
            {
                { TeamId.TEAM_BLUE, new Dictionary<LaneID, List<ILaneTurret>>{{LaneID.TOP, new List<ILaneTurret>()}, {LaneID.MIDDLE, new List<ILaneTurret>()}, {LaneID.BOTTOM, new List<ILaneTurret>()} } },
                { TeamId.TEAM_PURPLE, new Dictionary<LaneID, List<ILaneTurret>>{{LaneID.TOP, new List<ILaneTurret>()}, {LaneID.MIDDLE, new List<ILaneTurret>()}, {LaneID.BOTTOM, new List<ILaneTurret>()} }
            }};

            _inhibitors = new Dictionary<TeamId, Dictionary<LaneID, List<IInhibitor>>>
            {
                { TeamId.TEAM_BLUE, new Dictionary<LaneID, List<IInhibitor>>{{LaneID.TOP, new List<IInhibitor>()}, {LaneID.MIDDLE, new List<IInhibitor>()}, {LaneID.BOTTOM, new List<IInhibitor>()} } },
                { TeamId.TEAM_PURPLE, new Dictionary<LaneID, List<IInhibitor>>{{LaneID.TOP, new List<IInhibitor>()}, {LaneID.MIDDLE, new List<IInhibitor>()}, {LaneID.BOTTOM, new List<IInhibitor>()} }
            }};

            // Below is where we create the buildings.

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
                        _turrets[teamId][LaneID.MIDDLE].Add(new LaneTurret(_game, mapObject.Name + "_A", MapScript.TowerModels[teamId][TurretType.FOUNTAIN_TURRET], position, teamId, TurretType.FOUNTAIN_TURRET, GetTurretItems(TurretType.FOUNTAIN_TURRET), 0, LaneID.MIDDLE, mapObject));
                        continue;
                    }

                    int index = mapObject.ParseIndex();

                    // Failed to find an index in the turret's name, skip it altogether since it would be invalid.
                    if (index == -1)
                    {
                        // TODO: Verify if we should still add them; they would be assigned to lane NONE as a fountain turret.
                        continue;
                    }

                    var turretType = MapScript.GetTurretType(index, lane, teamId);

                    if (turretType == TurretType.FOUNTAIN_TURRET)
                    {
                        missedTurrets.Add(mapObject);
                        continue;
                    }

                    // index - 1 as we need it to start at 0.
                    _turrets[teamId][lane].Add(new LaneTurret(_game, mapObject.Name + "_A", MapScript.TowerModels[teamId][turretType], position, teamId, turretType, GetTurretItems(turretType), 0, lane, mapObject));
                }
                else if (objectType == GameObjectTypes.ObjBuilding_SpawnPoint)
                {
                    AddFountain(teamId, position);
                }
            }
        }
        //Currently towers are spawned by the Protection system, I think having 2 separate systems in the future might be ideal
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

            TurretType inhibProtection = TurretType.INNER_TURRET;

            //If the map doesn't have Inner turrets, the outer turrets will be used for inhibitor protection instead
            if (!MapScript.HasInnerTurrets)
            {
                inhibProtection = TurretType.OUTER_TURRET;
            }

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
                        _game.ProtectionManager.AddProtection(turret, false, _turrets[inhibitor.Team][inhibitor.Lane].First(dependTurret => dependTurret.Type == inhibProtection));
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
        //The way the turret spawning is handled above is based on the inhibitor lanes, so for example, if there's no mid inhibitor, no midlane towers would be spawned. So this is so we can spawn them manually
        public void SpawnTurret(ILaneTurret turret, bool hasProtection, bool protectionDependsOfAll = false, IAttackableUnit[] protectedBy = null)
        {
            if (hasProtection && protectedBy != null)
            {
                _game.ProtectionManager.AddProtection(turret, protectionDependsOfAll, protectedBy);
            }
            _game.ObjectManager.AddObject(turret);
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
        public bool IsMinionSpawnEnabled()
        {
            return _game.Config.MinionSpawnsEnabled;
        }
        public bool Spawn()
        {
            var spawnToWaypoints = new Dictionary<string, Tuple<List<Vector2>, uint>>();
            foreach (var barrack in _mapData.SpawnBarracks)
            {
                TeamId opposed_team = barrack.Value.GetOpposingTeamID();
                LaneID lane = barrack.Value.GetSpawnBarrackLaneID();
                List<Vector2> waypoint = MapScript.MinionPaths[barrack.Value.GetTeamID()][lane];

                spawnToWaypoints.Add(barrack.Value.Name, Tuple.Create(waypoint, _inhibitors[opposed_team][lane][0].NetId));
            }

            int cannonMinionCap = 2;
            foreach (var barrack in _mapData.SpawnBarracks)
            {
                //Howling Abyss has some odd minion spawn files, maybe used for poros' spawning?
                if (!barrack.Value.Name.StartsWith("____P"))
                {
                    continue;
                }
                var waypoints = spawnToWaypoints[barrack.Value.Name].Item1;
                var inhibitorId = spawnToWaypoints[barrack.Value.Name].Item2;
                var inhibitor = GetInhibitorById(inhibitorId);
                var isInhibitorDead = inhibitor.InhibitorState == InhibitorState.DEAD && !inhibitor.RespawnAnnounced;

                var oppositeTeam = TeamId.TEAM_BLUE;
                if (inhibitor.Team == TeamId.TEAM_PURPLE)
                {
                    oppositeTeam = TeamId.TEAM_PURPLE;
                }

                var areAllInhibitorsDead = AllInhibitorsDestroyedFromTeam(oppositeTeam) && !inhibitor.RespawnAnnounced;

                var spawnWave = MapScript.MinionWaveToSpawn(_game.GameTime, _cannonMinionCount, isInhibitorDead, areAllInhibitorsDead);
                cannonMinionCap = spawnWave.Item1;

                SpawnMinion(spawnWave.Item2, _minionNumber, barrack.Value.Name, waypoints);
            }

            if (_minionNumber < 8)
            {
                return false;
            }

            if (_cannonMinionCount >= cannonMinionCap)
            {
                _cannonMinionCount = 0;
            }
            else
            {
                _cannonMinionCount++;
            }
            return true;
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
        public void HandleSurrender(int userId, IChampion who, bool vote)
        {
            if (_surrenders.ContainsKey(who.Team))
                _surrenders[who.Team].HandleSurrender(userId, who, vote);
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
