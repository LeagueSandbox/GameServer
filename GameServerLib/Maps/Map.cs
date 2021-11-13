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
        public List<Tuple<IMinion, uint>> _capturePoints = new List<Tuple<IMinion, uint>>();
        public Dictionary<LaneID, List<Vector2>> BlueMinionPathing;
        public Dictionary<LaneID, List<Vector2>> PurpleMinionPathing;

        /// <summary>
        /// Instantiates map related game settings such as collision handler, navigation grid, announcer events, and map properties.
        /// </summary>
        /// <param name="game">Game instance.</param>
        public Map(Game game)
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

            AnnouncerEvents = new List<IAnnounce>();
            CollisionHandler = new CollisionHandler(this);

            MapScript = _scriptEngine.CreateObject<IMapScript>("MapScripts", $"Map{Id}") ?? new EmptyMapScript();
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
            if (MapScript.EnableBuildingProtection)
            {
                LoadBuildingProtection();
            }
            SpawnBuildings();
        }

        public void LoadBuildings()
        {
            _turrets = new Dictionary<TeamId, Dictionary<LaneID, List<ILaneTurret>>>{
                { TeamId.TEAM_BLUE, new Dictionary<LaneID, List<ILaneTurret>>{ { LaneID.NONE, new List<ILaneTurret>() },{ LaneID.TOP, new List<ILaneTurret>()}, {LaneID.MIDDLE, new List<ILaneTurret>()}, {LaneID.BOTTOM, new List<ILaneTurret>()} } },
                { TeamId.TEAM_PURPLE, new Dictionary<LaneID, List<ILaneTurret>>{ { LaneID.NONE, new List<ILaneTurret>() }, { LaneID.TOP, new List<ILaneTurret>()}, {LaneID.MIDDLE, new List<ILaneTurret>()}, {LaneID.BOTTOM, new List<ILaneTurret>()} } },
                { TeamId.TEAM_NEUTRAL, new Dictionary<LaneID, List<ILaneTurret>>{ { LaneID.NONE, new List<ILaneTurret>() }, { LaneID.TOP, new List<ILaneTurret>()}, {LaneID.MIDDLE, new List<ILaneTurret>()}, {LaneID.BOTTOM, new List<ILaneTurret>()} }}};

            _inhibitors = new Dictionary<TeamId, Dictionary<LaneID, List<IInhibitor>>>{
                { TeamId.TEAM_BLUE, new Dictionary<LaneID, List<IInhibitor>>{{LaneID.TOP, new List<IInhibitor>()}, {LaneID.MIDDLE, new List<IInhibitor>()}, {LaneID.BOTTOM, new List<IInhibitor>()} } },
                { TeamId.TEAM_PURPLE, new Dictionary<LaneID, List<IInhibitor>>{{LaneID.TOP, new List<IInhibitor>()}, {LaneID.MIDDLE, new List<IInhibitor>()}, {LaneID.BOTTOM, new List<IInhibitor>()} } },
                { TeamId.TEAM_NEUTRAL, new Dictionary<LaneID, List<IInhibitor>>{{LaneID.TOP, new List<IInhibitor>()}, {LaneID.MIDDLE, new List<IInhibitor>()}, {LaneID.BOTTOM, new List<IInhibitor>()} }}};

            BlueMinionPathing = new Dictionary<LaneID, List<Vector2>> { { LaneID.NONE, new List<Vector2>() }, { LaneID.TOP, new List<Vector2>() }, { LaneID.MIDDLE, new List<Vector2>() }, { LaneID.BOTTOM, new List<Vector2>() } };
            PurpleMinionPathing = new Dictionary<LaneID, List<Vector2>> { { LaneID.NONE, new List<Vector2>() }, { LaneID.TOP, new List<Vector2>() }, { LaneID.MIDDLE, new List<Vector2>() }, { LaneID.BOTTOM, new List<Vector2>() } };

            // Below is where we create the buildings.
            var inhibRadius = 214;
            var nexusRadius = 353;
            var sightRange = 1700;
            Dictionary<TeamId, List<IMapObject>> test = new Dictionary<TeamId, List<IMapObject>> { { TeamId.TEAM_BLUE, new List<IMapObject>() }, { TeamId.TEAM_PURPLE, new List<IMapObject>() } };
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
                if (objectType == GameObjectTypes.ObjAnimated_HQ || (teamId != TeamId.TEAM_NEUTRAL && mapObject.Name == MapScript.NexusModels[teamId]))
                {
                    //Nexus model changes dont seem to take effect in-game
                    _nexus.Add(new Nexus(_game, MapScript.NexusModels[teamId], teamId, nexusRadius, position, sightRange, Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(mapObject.Name)) | 0xFF000000));
                }
                // Inhibitors
                else if (objectType == GameObjectTypes.ObjAnimated_BarracksDampener)
                {
                    //Inhibitor model changes dont seem to take effect in-game
                    _inhibitors[teamId][lane].Add(new Inhibitor(_game, MapScript.InhibitorModels[teamId], lane, teamId, inhibRadius, position, sightRange, Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(mapObject.Name)) | 0xFF000000));
                    test[teamId].Add(mapObject);
                }
                // Turrets
                else if (objectType == GameObjectTypes.ObjAIBase_Turret)
                {
                    if (mapObject.Name.Contains("Shrine"))
                    {
                        _turrets[teamId][lane].Add(new LaneTurret(_game, mapObject.Name + "_A", MapScript.TowerModels[teamId][TurretType.FOUNTAIN_TURRET], position, teamId, TurretType.FOUNTAIN_TURRET, GetTurretItems(TurretType.FOUNTAIN_TURRET), 0, LaneID.NONE, mapObject));
                        continue;
                    }

                    int index = mapObject.ParseIndex();

                    var turretType = MapScript.GetTurretType(index, lane, teamId);

                    if (turretType == TurretType.FOUNTAIN_TURRET)
                    {
                        missedTurrets.Add(mapObject);
                        continue;
                    }

                    if (mapObject.Name.Contains("_Point"))
                    {
                        _capturePoints.Add(new Tuple<IMinion, uint>(new Minion(_game, null, new Vector2(mapObject.CentralPoint.X, mapObject.CentralPoint.Z), "OdinNeutralGuardian", "OdinNeutralGuardian"), _game.NetworkIdManager.GetNewNetId()));
                        continue;
                    }
                    _turrets[teamId][lane].Add(new LaneTurret(_game, mapObject.Name + "_A", MapScript.TowerModels[teamId][turretType], position, teamId, turretType, GetTurretItems(turretType), 0, lane, mapObject));
                }
                else if (objectType == GameObjectTypes.ObjBuilding_SpawnPoint)
                {
                    AddFountain(teamId, position);
                }
                else if (objectType == GameObjectTypes.ObjBuilding_NavPoint)
                {
                    BlueMinionPathing[lane].Add(new Vector2(mapObject.CentralPoint.X, mapObject.CentralPoint.Z));
                }
            }

            //If the map doesn't have any Minion pathing file but the map script has Minion pathing hardcoded
            if (BlueMinionPathing.Count == 0 && MapScript.MinionPaths != null && MapScript.MinionPaths.Count != 0 || MapScript.MinionPathingOverride)
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
                var SpawnBarracks = MapData.SpawnBarracks.Values.ToList().FindAll(x => x.GetLaneID() == lane);
                foreach (var SpawnBarrack in SpawnBarracks)
                {
                    if (SpawnBarrack.GetTeamID() == TeamId.TEAM_PURPLE)
                    {
                        BlueMinionPathing[lane].Add(new Vector2(SpawnBarrack.CentralPoint.X, SpawnBarrack.CentralPoint.Z));
                    }
                    else
                    {
                        PurpleMinionPathing[lane].Add(new Vector2(SpawnBarrack.CentralPoint.X, SpawnBarrack.CentralPoint.Z));
                    }
                }
            }
        }
        //Spawn Buildings
        public void SpawnBuildings()
        {
            //Spawn Nexus
            foreach (var nexus in _nexus)
            {
                _game.ObjectManager.AddObject(nexus);
            }
            foreach (var team in _inhibitors.Keys)
            {
                foreach (var lane in _inhibitors[team].Keys)
                {
                    //Spawn Inhibitors
                    foreach (var inhibitor in _inhibitors[team][lane])
                    {
                        _game.ObjectManager.AddObject(inhibitor);
                    }
                    //Spawn Turrets
                    foreach (var turret in _turrets[team][lane])
                    {
                        // Adds Turrets
                        _game.ObjectManager.AddObject(turret);
                    }
                }
                //Spawn FountainTurrets
                foreach (var turret in _turrets[team][LaneID.NONE])
                {
                    // Adds FountainTurret
                    _game.ObjectManager.AddObject(turret);
                }
            }
        }
        //Load Building Protections
        public void LoadBuildingProtection()
        {
            //I can't help but feel there's a better way to do this
            Dictionary<TeamId, List<IInhibitor>> TeamInhibitors = new Dictionary<TeamId, List<IInhibitor>> { { TeamId.TEAM_BLUE, new List<IInhibitor>() }, { TeamId.TEAM_PURPLE, new List<IInhibitor>() } };
            foreach (var teams in _inhibitors.Keys)
            {
                foreach (var lane in _inhibitors[teams].Keys)
                {
                    foreach (var inhibs in _inhibitors[teams][lane])
                    {
                        TeamInhibitors[teams].Add(inhibs);
                    }
                }
            }

            foreach (var nexus in _nexus)
            {
                // Adds Protection to Nexus
                _game.ProtectionManager.AddProtection
                (
                    nexus,
                    _turrets[nexus.Team][LaneID.MIDDLE].FindAll(turret => turret.Type == TurretType.NEXUS_TURRET).ToArray(), TeamInhibitors[nexus.Team].ToArray()

                );
                var teste = TeamInhibitors[nexus.Team];
            }

            TurretType inhibProtection = TurretType.INNER_TURRET;
            //If the map doesn't have Inner turrets, the outer turrets will be used for inhibitor protection instead
            if (!MapScript.HasInnerTurrets)
            {
                inhibProtection = TurretType.OUTER_TURRET;
            }

            foreach (var InhibTeam in TeamInhibitors.Keys)
            {
                foreach (var inhibitor in TeamInhibitors[InhibTeam])
                {
                    var inhibitorTurret = _turrets[inhibitor.Team][inhibitor.Lane].First(turret => turret.Type == TurretType.INHIBITOR_TURRET);

                    // Adds Protection to Inhibitors
                    if (inhibitorTurret != null)
                    {
                        // Depends on the first available inhibitor turret.
                        _game.ProtectionManager.AddProtection(inhibitor, false, inhibitorTurret);
                    }

                    // Adds Protection to Turrets
                    foreach (var turret in _turrets[inhibitor.Team][inhibitor.Lane])
                    {
                        if (turret.Type == TurretType.NEXUS_TURRET)
                        {
                            _game.ProtectionManager.AddProtection(turret, false, TeamInhibitors[inhibitor.Team].ToArray());
                        }
                        else if (turret.Type == TurretType.INHIBITOR_TURRET)
                        {
                            _game.ProtectionManager.AddProtection(turret, false, _turrets[inhibitor.Team][inhibitor.Lane].First(dependTurret => dependTurret.Type == inhibProtection));
                        }
                        else if (turret.Type == TurretType.INNER_TURRET)
                        {
                            _game.ProtectionManager.AddProtection(turret, false, _turrets[inhibitor.Team][inhibitor.Lane].First(dependTurret => dependTurret.Type == TurretType.OUTER_TURRET));
                        }
                    }
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
        public void SpawnCapturePoints()
        {
            foreach (var point in _capturePoints)
            {
                new Region(_game, TeamId.TEAM_BLUE, point.Item1.Position, -2, point.Item1, giveVision: true, visionRadius: 800.0f, revealStealth: true, hasCollision: true, collisionRadius: 120.0f, grassRadius: 150.0f, lifetime: 25000.0f);
                //_game.PacketNotifier.NotifyAddRegion(point.Item1.NetId, point.Item2, TeamId.TEAM_BLUE, point.Item1.Position, 25000.0f, 800.0f, -2, collisionRadius: 120.0f, grassRadius: 150.0f, stealthVis: true);
                point.Item1.PauseAi(true);
                _game.ObjectManager.AddObject(point.Item1);
            }
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
            var coords = MapData.SpawnBarracks[spawnPosition].CentralPoint;

            var teamID = TeamId.TEAM_BLUE;
            if (spawnPosition.Contains("Chaos"))
            {
                teamID = TeamId.TEAM_PURPLE;
            }
            return new Tuple<TeamId, Vector2>(teamID, new Vector2(coords.X, coords.Z));
        }

        public void SpawnLaneMinion(List<MinionSpawnType> list, int minionNo, string barracksName, List<Vector2> waypoints)
        {
            if (list.Count <= minionNo)
            {
                return;
            }

            var team = GetMinionSpawnPosition(barracksName).Item1;
            var m = new LaneMinion(_game, list[minionNo], barracksName, waypoints, MapScript.MinionModels[team][list[minionNo]], 0, team);
            _game.ObjectManager.AddObject(m);
        }
        public IMinion CreateMinion(string name, string model, Vector2 position, uint netId = 0, TeamId team = TeamId.TEAM_NEUTRAL, int skinId = 0, bool ignoreCollision = false, bool isTargetable = false)
        {
            var m = new Minion(_game, null, position, model, name, netId, team, skinId, ignoreCollision, isTargetable);
            _game.PacketNotifier.NotifySpawn(m);
            return m;
        }
        public void SpawnMinion(IMinion minion)
        {
            _game.ObjectManager.AddObject(minion);
        }
        public bool IsMinionSpawnEnabled()
        {
            return _game.Config.MinionSpawnsEnabled;
        }

        public bool Spawn()
        {
            var spawnToWaypoints = new Dictionary<string, Tuple<List<Vector2>, uint>>();
            foreach (var barrack in MapData.SpawnBarracks)
            {
                TeamId opposed_team = barrack.Value.GetOpposingTeamID();
                TeamId barrackTeam = barrack.Value.GetTeamID();
                LaneID lane = barrack.Value.GetSpawnBarrackLaneID();
                List<Vector2> waypoint = new List<Vector2>();

                if (barrackTeam == TeamId.TEAM_BLUE)
                {
                    waypoint = BlueMinionPathing[lane];
                }
                else if (barrackTeam == TeamId.TEAM_PURPLE)
                {
                    waypoint = PurpleMinionPathing[lane];
                }

                spawnToWaypoints.Add(barrack.Value.Name, Tuple.Create(waypoint, _inhibitors[opposed_team][lane][0].NetId));
            }

            int cannonMinionCap = 2;
            foreach (var barrack in MapData.SpawnBarracks)
            {
                var waypoints = spawnToWaypoints[barrack.Value.Name].Item1;
                var inhibitorId = spawnToWaypoints[barrack.Value.Name].Item2;
                var inhibitor = GetInhibitorById(inhibitorId);
                var isInhibitorDead = inhibitor.InhibitorState == InhibitorState.DEAD && !inhibitor.RespawnAnnounced;

                var oppositeTeam = barrack.Value.GetOpposingTeamID();

                var areAllInhibitorsDead = AllInhibitorsDestroyedFromTeam(oppositeTeam) && !inhibitor.RespawnAnnounced;

                var spawnWave = MapScript.MinionWaveToSpawn(_game.GameTime, _cannonMinionCount, isInhibitorDead, areAllInhibitorsDead);
                cannonMinionCap = spawnWave.Item1;

                SpawnLaneMinion(spawnWave.Item2, _minionNumber, barrack.Value.Name, waypoints);
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
        public IRegion CreateRegion(TeamId team, Vector2 position, int type = -1, IGameObject collisionUnit = null, IGameObject visionTarget = null, bool giveVision = false, float visionRadius = 0, bool revealStealth = false, bool hasColision = false, float colisionRadius = 0, float grassRadius = 0, float scale = 1, float addedSize = 0, float lifeTime = 0, int clientID = 0)
        {
           return new Region(_game, team, position, type, collisionUnit, visionTarget, giveVision, visionRadius, revealStealth, hasColision, colisionRadius, grassRadius, scale, addedSize, lifeTime, clientID);
        }
        public void AddAnnouncement(long time, Announces ID, bool IsMapSpecific)
        {
            AnnouncerEvents.Add(new Announce(_game, time, ID, IsMapSpecific));
        }
        public void AddLevelProp(string name, string model, Vector2 position, float height, Vector3 direction, Vector3 posOffset, Vector3 scale, int skinId = 0, byte skillLevel = 0, byte rank = 0, byte type = 0, uint netId = 0, byte netNodeId = 0)
        {
            _game.ObjectManager.AddObject(new LevelProp(_game, netNodeId, name, model, position, height, direction, posOffset, scale, skinId, skillLevel, rank, type, netId));
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
