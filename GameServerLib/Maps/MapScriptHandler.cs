using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using Force.Crc32;
using GameServerCore;
using GameServerCore.Content;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Maps;
using GameServerCore.NetInfo;
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
        /// Gamemode name designated by the game config. Determines which MapScript to load.
        /// </summary>
        public string GameMode { get; private set; }
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


        public Dictionary<LaneID, List<Vector2>> BlueMinionPathing;
        public Dictionary<LaneID, List<Vector2>> PurpleMinionPathing;
        public Dictionary<string, IMapObject> SpawnBarracks { get; set; }
        public List<INexus> NexusList { get; set; } = new List<INexus>();
        public List<IMapObject> InfoPoints { get; set; } = new List<IMapObject>();
        public Dictionary<TeamId, Dictionary<LaneID, List<ILaneTurret>>> TurretList { get; set; }
        public Dictionary<TeamId, Dictionary<LaneID, List<IInhibitor>>> InhibitorList { get; set; }
        public Dictionary<TeamId, IFountain> FountainList { get; set; } = new Dictionary<TeamId, IFountain>();
        public Dictionary<TeamId, IGameObject> ShopList { get; set; } = new Dictionary<TeamId, IGameObject>();
        public Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> PlayerSpawnPoints { get; set; } = new Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>>();

        private int _minionNumber;
        private int _cannonMinionCount;
        private readonly Dictionary<TeamId, SurrenderHandler> _surrenders = new Dictionary<TeamId, SurrenderHandler>();

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
            GameMode = game.Config.GameConfig.GameMode;
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

            if (String.IsNullOrEmpty(GameMode))
            {
                _logger.Error("No GameMode Specified, Defaulting to CLASSIC...");
                GameMode = "CLASSIC";
            }
            MapScript = _scriptEngine.CreateObject<IMapScript>($"MapScripts.Map{Id}", $"{GameMode}") ?? new EmptyMapScript();

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
            foreach (var surrender in _surrenders.Values)
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

            MapScript.Update(diff);
        }

        /// <summary>
        /// Initializes MapProperties. Usually only occurs once before players are added to Game.
        /// </summary>
        public void Init()
        {
            LoadBuildings();
            MapScript.Init(this);
            if (MapScript.MapScriptMetadata.EnableBuildingProtection)
            {
                LoadBuildingProtection();
            }
            SpawnBuildings();
        }

        public void LoadBuildings()
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

            // Below is where we create the buildings.
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
                if (objectType == GameObjectTypes.ObjAnimated_HQ || (teamId != TeamId.TEAM_NEUTRAL && mapObject.Name == MapScript.NexusModels[teamId]))
                {
                    //Nexus model changes dont seem to take effect in-game
                    NexusList.Add(new Nexus(_game, MapScript.NexusModels[teamId], teamId, nexusRadius, position, sightRange, Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(mapObject.Name)) | 0xFF000000));
                }
                // Inhibitors
                else if (objectType == GameObjectTypes.ObjAnimated_BarracksDampener)
                {
                    //Inhibitor model changes dont seem to take effect in-game
                    InhibitorList[teamId][lane].Add(new Inhibitor(_game, MapScript.InhibitorModels[teamId], lane, teamId, inhibRadius, position, sightRange, Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(mapObject.Name)) | 0xFF000000));
                }
                // Turrets
                else if (objectType == GameObjectTypes.ObjAIBase_Turret)
                {
                    if (mapObject.Name.Contains("Shrine"))
                    {
                        TurretList[teamId][lane].Add(new LaneTurret(_game, mapObject.Name + "_A", MapScript.TowerModels[teamId][TurretType.FOUNTAIN_TURRET], position, teamId, TurretType.FOUNTAIN_TURRET, GetTurretItems(TurretType.FOUNTAIN_TURRET), 0, LaneID.NONE, mapObject, MapScript.LaneTurretAI));
                        continue;
                    }

                    int index = mapObject.ParseIndex();

                    var turretType = MapScript.GetTurretType(index, lane, teamId);

                    if (turretType == TurretType.FOUNTAIN_TURRET)
                    {
                        missedTurrets.Add(mapObject);
                        continue;
                    }

                    TurretList[teamId][lane].Add(new LaneTurret(_game, mapObject.Name + "_A", MapScript.TowerModels[teamId][turretType], position, teamId, turretType, GetTurretItems(turretType), 0, lane, mapObject, MapScript.LaneTurretAI));
                }
                else if (objectType == GameObjectTypes.InfoPoint)
                {
                    InfoPoints.Add(mapObject);
                }
                else if (objectType == GameObjectTypes.ObjBuilding_SpawnPoint)
                {
                    AddFountain(teamId, position);
                }
                else if (objectType == GameObjectTypes.ObjBuilding_NavPoint)
                {
                    BlueMinionPathing[lane].Add(new Vector2(mapObject.CentralPoint.X, mapObject.CentralPoint.Z));
                }
                else if (objectType == GameObjectTypes.ObjBuilding_Shop)
                {
                    ShopList.Add(teamId, new GameObject(_game, position, netId: Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(mapObject.Name)) | 0xFF000000, team: teamId));
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
        //Spawn Buildings
        public void SpawnBuildings()
        {
            //Spawn Nexus
            foreach (var nexus in NexusList)
            {
                _game.ObjectManager.AddObject(nexus);
            }
            foreach (var team in InhibitorList.Keys)
            {
                foreach (var lane in InhibitorList[team].Keys)
                {
                    //Spawn Inhibitors
                    foreach (var inhibitor in InhibitorList[team][lane])
                    {
                        _game.ObjectManager.AddObject(inhibitor);
                    }
                    //Spawn Turrets
                    foreach (var turret in TurretList[team][lane])
                    {
                        // Adds Turrets
                        _game.ObjectManager.AddObject(turret);
                    }
                }
                //Spawn FountainTurrets
                foreach (var turret in TurretList[team][LaneID.NONE])
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
            foreach (var teams in InhibitorList.Keys)
            {
                foreach (var lane in InhibitorList[teams].Keys)
                {
                    foreach (var inhibs in InhibitorList[teams][lane])
                    {
                        TeamInhibitors[teams].Add(inhibs);
                    }
                }
            }

            foreach (var nexus in NexusList)
            {
                // Adds Protection to Nexus
                _game.ProtectionManager.AddProtection(nexus, TurretList[nexus.Team][LaneID.MIDDLE].FindAll(turret => turret.Type == TurretType.NEXUS_TURRET).ToArray(), TeamInhibitors[nexus.Team].ToArray());
            }

            foreach (var InhibTeam in TeamInhibitors.Keys)
            {
                foreach (var inhibitor in TeamInhibitors[InhibTeam])
                {
                    var inhibitorTurret = TurretList[inhibitor.Team][inhibitor.Lane].First(turret => turret.Type == TurretType.INHIBITOR_TURRET);

                    // Adds Protection to Inhibitors
                    if (inhibitorTurret != null)
                    {
                        // Depends on the first available inhibitor turret.
                        _game.ProtectionManager.AddProtection(inhibitor, false, inhibitorTurret);
                    }

                    // Adds Protection to Turrets
                    foreach (var turret in TurretList[inhibitor.Team][inhibitor.Lane])
                    {
                        if (turret.Type == TurretType.NEXUS_TURRET)
                        {
                            _game.ProtectionManager.AddProtection(turret, false, TeamInhibitors[inhibitor.Team].ToArray());
                        }
                        else if (turret.Type == TurretType.INHIBITOR_TURRET)
                        {
                            _game.ProtectionManager.AddProtection(turret, false, TurretList[inhibitor.Team][inhibitor.Lane].First(dependTurret => dependTurret.Type == TurretType.INNER_TURRET));
                        }
                        else if (turret.Type == TurretType.INNER_TURRET)
                        {
                            //Checks if there are outer turrets
                            if (TurretList[inhibitor.Team][inhibitor.Lane].Any(outerTurret => outerTurret.Type == TurretType.OUTER_TURRET))
                            {
                                _game.ProtectionManager.AddProtection(turret, false, TurretList[inhibitor.Team][inhibitor.Lane].First(dependTurret => dependTurret.Type == TurretType.OUTER_TURRET));
                            }
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
            var tower = TurretList[team][currentLaneId].Find(x => x.Name == towerName);
            tower.SetLaneID(desiredLaneID);
            TurretList[team][currentLaneId].Remove(tower);
            TurretList[team][desiredLaneID].Add(tower);
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
            foreach (TeamId team in InhibitorList.Keys)
            {
                foreach (LaneID lane in InhibitorList[team].Keys)
                {
                    foreach (var inhibitor in InhibitorList[team][lane])
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
            foreach (LaneID lane in InhibitorList[team].Keys)
            {
                foreach (var inhibitor in InhibitorList[team][lane])
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
            var coords = SpawnBarracks[spawnPosition].CentralPoint;

            var teamID = TeamId.TEAM_BLUE;
            if (spawnPosition.Contains("Chaos"))
            {
                teamID = TeamId.TEAM_PURPLE;
            }
            return new Tuple<TeamId, Vector2>(teamID, new Vector2(coords.X, coords.Z));
        }

        public void CreateLaneMinion(List<MinionSpawnType> list, int minionNo, string barracksName, List<Vector2> waypoints)
        {
            if (list.Count <= minionNo)
            {
                return;
            }

            var team = GetMinionSpawnPosition(barracksName).Item1;
            var m = new LaneMinion(_game, list[minionNo], barracksName, waypoints, MapScript.MinionModels[team][list[minionNo]], 0, team, MapScript.LaneMinionAI);
            _game.ObjectManager.AddObject(m);
        }
        public IMinion CreateMinion(
            string name, string model, Vector2 position, uint netId = 0,
            TeamId team = TeamId.TEAM_NEUTRAL, int skinId = 0, bool ignoreCollision = false,
            bool isTargetable = false, string aiScript = "", int damageBonus = 0,
            int healthBonus = 0, int initialLevel = 1)
        {
            var m = new Minion(_game, null, position, model, name, netId, team, skinId, ignoreCollision, isTargetable, null, aiScript, damageBonus, healthBonus, initialLevel);
            _game.ObjectManager.AddObject(m);
            return m;
        }
        public void AddObject(IGameObject obj)
        {
            _game.ObjectManager.AddObject(obj);
        }
        public bool IsMinionSpawnEnabled()
        {
            return _game.Config.GameFeatures.HasFlag(FeatureFlags.EnableLaneMinions);
        }

        public bool SetUpLaneMinion()
        {
            int cannonMinionCap = 2;
            foreach (var barrack in SpawnBarracks)
            {
                List<Vector2> waypoint = new List<Vector2>();
                TeamId opposed_team = barrack.Value.GetOpposingTeamID();
                TeamId barrackTeam = barrack.Value.GetTeamID();
                LaneID lane = barrack.Value.GetSpawnBarrackLaneID();
                IInhibitor inhibitor = InhibitorList[opposed_team][lane][0];
                bool isInhibitorDead = inhibitor.InhibitorState == InhibitorState.DEAD && !inhibitor.RespawnAnnounced;
                bool areAllInhibitorsDead = AllInhibitorsDestroyedFromTeam(opposed_team) && !inhibitor.RespawnAnnounced;
                Tuple<int, List<MinionSpawnType>> spawnWave = MapScript.MinionWaveToSpawn(_game.GameTime, _cannonMinionCount, isInhibitorDead, areAllInhibitorsDead);
                cannonMinionCap = spawnWave.Item1;

                if (barrackTeam == TeamId.TEAM_BLUE)
                {
                    waypoint = BlueMinionPathing[lane];
                }
                else if (barrackTeam == TeamId.TEAM_PURPLE)
                {
                    waypoint = PurpleMinionPathing[lane];
                }

                CreateLaneMinion(spawnWave.Item2, _minionNumber, barrack.Value.Name, waypoint);
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
        public IRegion CreateRegion(TeamId team, Vector2 position, RegionType type = RegionType.Default, IGameObject collisionUnit = null, IGameObject visionTarget = null, bool giveVision = false, float visionRadius = 0, bool revealStealth = false, bool hasCollision = false, float collisionRadius = 0, float grassRadius = 0, float scale = 1, float addedSize = 0, float lifeTime = 0, int clientID = 0)
        {
            return new Region(_game, team, position, type, collisionUnit, visionTarget, giveVision, visionRadius, revealStealth, hasCollision, collisionRadius, grassRadius, scale, addedSize, lifeTime, clientID);
        }
        public void AddAnnouncement(long time, EventID ID, bool isMapSpecific)
        {
            AnnouncerEvents.Add(new Announce(_game, time, ID, isMapSpecific));
        }
        public void AddLevelProp(string name, string model, Vector2 position, float height, Vector3 direction, Vector3 posOffset, Vector3 scale, int skinId = 0, byte skillLevel = 0, byte rank = 0, byte type = 2, uint netId = 0, byte netNodeId = 64)
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
            FountainList.Add(team, new Fountain(_game, team, position, 1000));
        }
        public void SetGameFeatures(FeatureFlags featureFlag, bool isEnabled)
        {
            _game.Config.SetGameFeatures(featureFlag, isEnabled);
        }
        //Game Time
        public float GameTime()
        {
            return _game.GameTime;
        }
        public void EndGame(TeamId losingTeam, Vector3 finalCameraPosition, float endGameTimer = 5000.0f, bool moveCamera = true, float cameraTimer = 3.0f, bool disableUI = true, IDeathData deathData = null)
        {
            //TODO: check if mapScripts should handle this directly
            var players = _game.PlayerManager.GetPlayers();
            _game.Stop();
            if (deathData != null)
            {
                _game.PacketNotifier.NotifyBuilding_Die(deathData);
            }
            _game.PacketNotifier.NotifyS2C_EndGame(losingTeam, endGameTimer);
            foreach (var player in players)
            {
                if (disableUI)
                {
                    _game.PacketNotifier.NotifyS2C_DisableHUDForEndOfGame(player);
                }
                if (moveCamera)
                {
                    _game.PacketNotifier.NotifyS2C_MoveCameraToPoint(player, Vector3.Zero, finalCameraPosition, cameraTimer);
                }
            }
            _game.SetGameToExit();
        }
    }
}
