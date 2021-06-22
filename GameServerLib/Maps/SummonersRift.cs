using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Force.Crc32;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Maps;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings;
using LeagueSandbox.GameServer.GameObjects.Other;
using LeagueSandbox.GameServer.Scripting.CSharp;

namespace LeagueSandbox.GameServer.Maps
{
    internal class SummonersRift : IMapProperties
    {
        private static readonly List<Vector2> BlueTopWaypoints = new List<Vector2>
        {
            new Vector2(917.0f, 1725.0f),
            new Vector2(1170.0f, 4041.0f),
            new Vector2(861.0f, 6459.0f),
            new Vector2(880.0f, 10180.0f),
            new Vector2(1268.0f, 11675.0f),
            new Vector2(2806.0f, 13075.0f),
            new Vector2(3907.0f, 13243.0f),
            new Vector2(7550.0f, 13407.0f),
            new Vector2(10244.0f, 13238.0f),
            new Vector2(10947.0f, 13135.0f),
            new Vector2(12511.0f, 12776.0f)
        };
        private static readonly List<Vector2> BlueBotWaypoints = new List<Vector2>
        {
            new Vector2(1487.0f, 1302.0f),
            new Vector2(3789.0f, 1346.0f),
            new Vector2(6430.0f, 1005.0f),
            new Vector2(10995.0f, 1234.0f),
            new Vector2(12841.0f, 3051.0f),
            new Vector2(13148.0f, 4202.0f),
            new Vector2(13249.0f, 7884.0f),
            new Vector2(12886.0f, 10356.0f),
            new Vector2(12511.0f, 12776.0f)
        };
        private static readonly List<Vector2> BlueMidWaypoints = new List<Vector2>
        {
            new Vector2(1418.0f, 1686.0f),
            new Vector2(2997.0f, 2781.0f),
            new Vector2(4472.0f, 4727.0f),
            new Vector2(8375.0f, 8366.0f),
            new Vector2(10948.0f, 10821.0f),
            new Vector2(12511.0f, 12776.0f)
        };
        private static readonly List<Vector2> RedTopWaypoints = new List<Vector2>
        {
            new Vector2(12451.0f, 13217.0f),
            new Vector2(10947.0f, 13135.0f),
            new Vector2(10244.0f, 13238.0f),
            new Vector2(7550.0f, 13407.0f),
            new Vector2(3907.0f, 13243.0f),
            new Vector2(2806.0f, 13075.0f),
            new Vector2(1268.0f, 11675.0f),
            new Vector2(880.0f, 10180.0f),
            new Vector2(861.0f, 6459.0f),
            new Vector2(1170.0f, 4041.0f),
            new Vector2(1418.0f, 1686.0f)
        };
        private static readonly List<Vector2> RedBotWaypoints = new List<Vector2>
        {
            new Vector2(13062.0f, 12760.0f),
            new Vector2(12886.0f, 10356.0f),
            new Vector2(13249.0f, 7884.0f),
            new Vector2(13148.0f, 4202.0f),
            new Vector2(12841.0f, 3051.0f),
            new Vector2(10995.0f, 1234.0f),
            new Vector2(6430.0f, 1005.0f),
            new Vector2(3789.0f, 1346.0f),
            new Vector2(1418.0f, 1686.0f)
        };
        private static readonly List<Vector2> RedMidWaypoints = new List<Vector2>
        {
            new Vector2(12511.0f, 12776.0f),
            new Vector2(10948.0f, 10821.0f),
            new Vector2(8375.0f, 8366.0f),
            new Vector2(4472.0f, 4727.0f),
            new Vector2(2997.0f, 2781.0f),
            new Vector2(1418.0f, 1686.0f)
        };

        private static readonly List<MinionSpawnType> RegularMinionWave = new List<MinionSpawnType>
        {
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER
        };
        private static readonly List<MinionSpawnType> CannonMinionWave = new List<MinionSpawnType>
        {
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CANNON,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER
        };
        private static readonly List<MinionSpawnType> SuperMinionWave = new List<MinionSpawnType>
        {
            MinionSpawnType.MINION_TYPE_SUPER,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER
        };
        private static readonly List<MinionSpawnType> DoubleSuperMinionWave = new List<MinionSpawnType>
        {
            MinionSpawnType.MINION_TYPE_SUPER,
            MinionSpawnType.MINION_TYPE_SUPER,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER
        };

        private static readonly Dictionary<TurretType, int[]> TurretItems = new Dictionary<TurretType, int[]>
        {
            { TurretType.OUTER_TURRET, new[] { 1500, 1501, 1502, 1503 } },
            { TurretType.INNER_TURRET, new[] { 1500, 1501, 1502, 1503, 1504 } },
            { TurretType.INHIBITOR_TURRET, new[] { 1501, 1502, 1503, 1505 } },
            { TurretType.NEXUS_TURRET, new[] { 1501, 1502, 1503, 1505 } }
        };

        private Game _game;
        private int _cannonMinionCount;
        private int _minionNumber;
        private readonly long _firstSpawnTime = 90 * 1000;
        private long _nextSpawnTime = 90 * 1000;
        private readonly long _spawnInterval = 30 * 1000;
        private readonly Dictionary<TeamId, Fountain> _fountains;
        private readonly List<Nexus> _nexus;
        private readonly Dictionary<TeamId, Dictionary<LaneID, List<Inhibitor>>> _inhibitors;
        private readonly Dictionary<TeamId, Dictionary<LaneID, List<LaneTurret>>> _turrets;
        private readonly Dictionary<TeamId, SurrenderHandler> _surrenders;
        private MapData _mapData;

        public float GoldPerSecond { get; set; } = 1.9f;
        public float StartingGold { get; set; } = 475.0f;
        public bool HasFirstBloodHappened { get; set; } = false;
        public bool IsKillGoldRewardReductionActive { get; set; } = true;
        public int BluePillId { get; set; } = 2001;
        public long FirstGoldTime { get; set; } = 90 * 1000;
        public bool SpawnEnabled { get; set; }

        public SummonersRift(Game game)
        {
            _game = game;
            _mapData = game.Config.MapData;
            _fountains = new Dictionary<TeamId, Fountain>
            {
                { TeamId.TEAM_BLUE, new Fountain(game, TeamId.TEAM_BLUE, new Vector2(11, 250), 1000) },
                { TeamId.TEAM_PURPLE, new Fountain(game, TeamId.TEAM_PURPLE, new Vector2(13950, 14200), 1000) }
            };
            _nexus = new List<Nexus>();
            _inhibitors = new Dictionary<TeamId, Dictionary<LaneID, List<Inhibitor>>>
            {
                {
                    TeamId.TEAM_BLUE, new Dictionary<LaneID, List<Inhibitor>>
                    {
                        { LaneID.MIDDLE, new List<Inhibitor>() },
                        { LaneID.TOP, new List<Inhibitor>() },
                        { LaneID.BOTTOM, new List<Inhibitor>() }
                    }
                },
                {
                    TeamId.TEAM_PURPLE, new Dictionary<LaneID, List<Inhibitor>>
                    {
                        { LaneID.MIDDLE, new List<Inhibitor>() },
                        { LaneID.TOP, new List<Inhibitor>() },
                        { LaneID.BOTTOM, new List<Inhibitor>() }
                    }
                }
            };
            _turrets = new Dictionary<TeamId, Dictionary<LaneID, List<LaneTurret>>>
            {
                {
                    TeamId.TEAM_BLUE, new Dictionary<LaneID, List<LaneTurret>>
                    {
                        { LaneID.MIDDLE, new List<LaneTurret>() },
                        { LaneID.TOP, new List<LaneTurret>() },
                        { LaneID.BOTTOM, new List<LaneTurret>() }
                    }
                },
                {
                    TeamId.TEAM_PURPLE, new Dictionary<LaneID, List<LaneTurret>>
                    {
                        { LaneID.MIDDLE, new List<LaneTurret>() },
                        { LaneID.TOP, new List<LaneTurret>() },
                        { LaneID.BOTTOM, new List<LaneTurret>() }
                    }
                }
            };
            _surrenders = new Dictionary<TeamId, SurrenderHandler>
            {
                { TeamId.TEAM_BLUE, new SurrenderHandler(game, TeamId.TEAM_BLUE, 1200000.0f , 300000.0f , 30.0f) },
                { TeamId.TEAM_PURPLE, new SurrenderHandler(game, TeamId.TEAM_PURPLE, 1200000.0f, 300000.0f, 30.0f) }
            };
            SpawnEnabled = _game.Config.MinionSpawnsEnabled;
        }

        public int[] GetTurretItems(TurretType type)
        {
            if (!TurretItems.ContainsKey(type))
            {
                return null;
            }

            return TurretItems[type];
        }

        public void AddTurret(MapData.MapObject parentObject, Vector2 position, TeamId team, string teamName, TurretType type, LaneID lane)
        {
            string towerModel = "";
            if (team == TeamId.TEAM_BLUE)
            {
                if (type == TurretType.FOUNTAIN_TURRET)
                {
                    towerModel = "TurretShrine";
                }
                else if (type == TurretType.NEXUS_TURRET)
                {
                    towerModel = "TurretAngel";
                }
                else if (type == TurretType.INHIBITOR_TURRET)
                {
                    towerModel = "TurretDragon";
                }
                else if (type == TurretType.INNER_TURRET)
                {
                    towerModel = "TurretNormal2";
                }
                else if (type == TurretType.OUTER_TURRET)
                {
                    towerModel = "TurretNormal";
                }
            }
            else
            {
                if (type == TurretType.FOUNTAIN_TURRET)
                {
                    towerModel = "TurretShrine";
                }
                else if (type == TurretType.NEXUS_TURRET)
                {
                    towerModel = "TurretGiant";
                }
                // Nexus and Inhib Towers Might be swapped, double check if that's right.
                else if (type == TurretType.INHIBITOR_TURRET)
                {
                    towerModel = "TurretNormal";
                }
                else if (type == TurretType.INNER_TURRET)
                {
                    towerModel = "TurretWorm2";
                }
                else if (type == TurretType.OUTER_TURRET)
                {
                    towerModel = "TurretWorm";
                }
            }

            _turrets[team][lane].Add(new LaneTurret(_game, parentObject.Name + "_A", teamName + towerModel, position, team, type, GetTurretItems(type), 0, lane, parentObject));
        }

        public TurretType GetTurretType(int trueIndex, LaneID lane)
        {
            TurretType returnType = TurretType.FOUNTAIN_TURRET;

            if (lane == LaneID.MIDDLE)
            {
                if (trueIndex < 3)
                {
                    returnType = TurretType.NEXUS_TURRET;
                    return returnType;
                }

                trueIndex -= 2;
            }

            if (trueIndex == 1)
            {
                returnType = TurretType.INHIBITOR_TURRET;
            }
            else if (trueIndex == 2)
            {
                returnType = TurretType.INNER_TURRET;
            }
            else if (trueIndex == 3)
            {
                returnType = TurretType.OUTER_TURRET;
            }

            return returnType;
        }

        public void Init()
        {
            // Announcer events
            _game.Map.AnnouncerEvents.Add(new Announce(_game, 30 * 1000, Announces.WELCOME_TO_SR, true)); // Welcome to SR
            if (_firstSpawnTime - 30 * 1000 >= 0.0f)
                _game.Map.AnnouncerEvents.Add(new Announce(_game, _firstSpawnTime - 30 * 1000, Announces.THIRY_SECONDS_TO_MINIONS_SPAWN, true)); // 30 seconds until minions spawn
            _game.Map.AnnouncerEvents.Add(new Announce(_game, _firstSpawnTime, Announces.MINIONS_HAVE_SPAWNED, false)); // Minions have spawned (90 * 1000)
            _game.Map.AnnouncerEvents.Add(new Announce(_game, _firstSpawnTime, Announces.MINIONS_HAVE_SPAWNED2, false)); // Minions have spawned [2] (90 * 1000)

            // TODO: Generate & use exact positions from content files

            //TODO: Unhardcode everything (preferably by reading from content)
            // These only matter for projectile collisions.
            var inhibRadius = 214;
            var nexusRadius = 353;
            var sightRange = 1700;

            // Below is where we create the buildings.

            // These two are used for fixing any wrongly indexed turrets that are present.
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
                }
                // Turrets
                else if (objectType == GameObjectTypes.ObjAIBase_Turret)
                {
                    if (mapObject.Name.Contains("Shrine"))
                    {
                        AddTurret(mapObject, position, teamId, teamName, TurretType.FOUNTAIN_TURRET, LaneID.MIDDLE);
                        continue;
                    }

                    int index = mapObject.ParseIndex();

                    // Failed to find an index in the turret's name, skip it altogether since it would be invalid.
                    if (index == -1)
                    {
                        // TODO: Verify if we should still add them; they would be assigned to lane NONE as a fountain turret.
                        continue;
                    }

                    var turretType = GetTurretType(index, lane);

                    if (turretType == TurretType.FOUNTAIN_TURRET)
                    {
                        missedTurrets.Add(mapObject);
                        continue;
                    }

                    // index - 1 as we need it to start at 0.
                    AddTurret(mapObject, position, teamId, teamName, turretType, lane);
                }
            }

            // Fix missed turrets (this is basically user-error handling).
            // Could be improved with pattern recognition rather than a set pattern.
            if (missedTurrets.Count > 0)
            {
                foreach (var missed in missedTurrets)
                {
                    TeamId teamId = missed.GetTeamID();

                    LaneID lane = missed.GetLaneID();
                    // the numbers here can unhardcoded when maps become fully scriptable, perhaps with a MapMetadata detailing the maximum turrets for each lane.
                    int[] maxLaneSizes = new int[] { 5, 3, 3 };

                    Vector2 position = new Vector2(missed.CentralPoint.X, missed.CentralPoint.Z);
                    // Models are specific to team.
                    string teamName = missed.GetTeamName();
                    TurretType turretType = TurretType.FOUNTAIN_TURRET;

                    int index = missed.ParseIndex();

                    // Failed to find an index in the turret's name.
                    if (index == -1 || lane == LaneID.NONE)
                    {
                        continue;
                    }

                    // Define whether each lane has been filled with the appropriate number of turrets, 
                    var midFinished = _turrets[teamId][LaneID.MIDDLE].Count - 1 >= maxLaneSizes[0];
                    var topFinished = _turrets[teamId][LaneID.TOP].Count - 1 >= maxLaneSizes[1];
                    var botFinished = _turrets[teamId][LaneID.BOTTOM].Count - 1 >= maxLaneSizes[2];

                    // Define the common pattern that the turrets are created in (unsure if this can/should be unhardcoded).
                    LaneID[] lanePattern = new LaneID[] { LaneID.MIDDLE, LaneID.TOP, LaneID.BOTTOM };
                    bool[] finishedPattern = new bool[] { midFinished, topFinished, botFinished };

                    // Compensates for the order of the missing turrets by using the distance from the last valid turret index in the same lane.
                    int missingTurretDistance = index - (_turrets[teamId][lane].Count - 1);

                    // The lanes have values which allow looping through the pattern.
                    int patternIndex = missingTurretDistance;

                    if (patternIndex >= lanePattern.Length)
                    {
                        // remainder from division is the new index.
                        patternIndex = patternIndex % lanePattern.Length;
                    }
                    else if (patternIndex < 0)
                    {
                        patternIndex = -patternIndex % lanePattern.Length;
                    }

                    // Get the lane after the current lane.
                    LaneID fixLane = lanePattern[patternIndex];

                    // If the next lane has finished, get the next lane from this finished lane.
                    if (finishedPattern[patternIndex])
                    {
                        fixLane = lanePattern[(int)fixLane];
                    }

                    if (fixLane != LaneID.NONE)
                    {
                        var fixLaneTurrets = _turrets[teamId][fixLane];
                        var firstIndex = fixLaneTurrets[0].ParentObject.ParseIndex();
                        var lastIndex = 0;

                        if (fixLaneTurrets.Count > 0)
                        {
                            lastIndex = fixLaneTurrets[fixLaneTurrets.Count - 1].ParentObject.ParseIndex();
                        }

                        // Grab the first or last available index.
                        var fixIndex = 0;
                        if (firstIndex > 1)
                        {
                            fixIndex = 1;
                        }
                        else if (lastIndex < maxLaneSizes[patternIndex])
                        {
                            fixIndex = lastIndex + 1;
                        }
                        else
                        {
                            // Skip turrets which cannot fit.
                            continue;
                        }

                        turretType = GetTurretType(fixIndex, fixLane);

                        // If it still failed, it is invalid.
                        if (turretType != TurretType.FOUNTAIN_TURRET)
                        {
                            AddTurret(missed, position, teamId, teamName, turretType, fixLane);
                        }
                    }
                }
            }
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

            //Map props
            _game.ObjectManager.AddObject(new LevelProp(_game, new Vector2(12465.0f, 14422.257f), 101.0f, new Vector3(0.0f, 0.0f, 0.0f), 0.0f, 0.0f, "LevelProp_Yonkey", "Yonkey"));
            _game.ObjectManager.AddObject(new LevelProp(_game, new Vector2(-76.0f, 1769.1589f), 94.0f, new Vector3(0.0f, 0.0f, 0.0f), 0.0f, 0.0f, "LevelProp_Yonkey1", "Yonkey"));
            _game.ObjectManager.AddObject(new LevelProp(_game, new Vector2(13374.17f, 14245.673f), 194.9741f, new Vector3(224.0f, 33.33f, 0.0f), 0.0f, -44.44f, "LevelProp_ShopMale", "ShopMale"));
            _game.ObjectManager.AddObject(new LevelProp(_game, new Vector2(-99.5613f, 855.6632f), 191.4039f, new Vector3(158.0f, 0.0f, 0.0f), 0.0f, 0.0f, "LevelProp_ShopMale1", "ShopMale"));
        }
        public void Update(float diff)
        {
            if (_game.GameTime >= 120 * 1000)
            {
                IsKillGoldRewardReductionActive = false;
            }

            if (SpawnEnabled)
            {
                if (_minionNumber > 0)
                {
                    if (_game.GameTime >= _nextSpawnTime + _minionNumber * 8 * 100)
                    { // Spawn new wave every 0.8s
                        if (Spawn())
                        {
                            _minionNumber = 0;
                            _nextSpawnTime = (long)_game.GameTime + _spawnInterval;
                        }
                        else
                        {
                            _minionNumber++;
                        }
                    }
                }
                else if (_game.GameTime >= _nextSpawnTime)
                {
                    Spawn();
                    _minionNumber++;
                }
            }

            foreach (var fountain in _fountains.Values)
            {
                fountain.Update(diff);
            }

            foreach (var surrender in _surrenders.Values)
                surrender.Update(diff);
        }

        public string GetMinionModel(TeamId team, MinionSpawnType type)
        {
            var teamDictionary = new Dictionary<TeamId, string>
            {
                {TeamId.TEAM_BLUE, "Blue"},
                {TeamId.TEAM_PURPLE, "Red"}
            };

            var typeDictionary = new Dictionary<MinionSpawnType, string>
            {
                {MinionSpawnType.MINION_TYPE_MELEE, "Basic"},
                {MinionSpawnType.MINION_TYPE_CASTER, "Wizard"},
                {MinionSpawnType.MINION_TYPE_CANNON, "MechCannon"},
                {MinionSpawnType.MINION_TYPE_SUPER, "MechMelee"}
            };

            if (!teamDictionary.ContainsKey(team) || !typeDictionary.ContainsKey(type))
            {
                return string.Empty;
            }

            return $"{teamDictionary[team]}_Minion_{typeDictionary[type]}";
        }

        public float GetGoldFor(IAttackableUnit u)
        {
            if (!(u is ILaneMinion m))
            {
                if (!(u is IChampion c))
                {
                    return 0.0f;
                }

                var gold = 300.0f; //normal gold for a kill
                if (c.KillDeathCounter < 5 && c.KillDeathCounter >= 0)
                {
                    if (c.KillDeathCounter == 0)
                    {
                        return gold;
                    }

                    for (var i = c.KillDeathCounter; i > 1; --i)
                    {
                        gold += gold * 0.165f;
                    }

                    return gold;
                }

                if (c.KillDeathCounter >= 5)
                {
                    return 500.0f;
                }

                if (c.KillDeathCounter >= 0)
                    return 0.0f;

                var firstDeathGold = gold - gold * 0.085f;

                if (c.KillDeathCounter == -1)
                {
                    return firstDeathGold;
                }

                for (var i = c.KillDeathCounter; i < -1; ++i)
                {
                    firstDeathGold -= firstDeathGold * 0.2f;
                }

                if (firstDeathGold < 50)
                {
                    firstDeathGold = 50;
                }

                return firstDeathGold;
            }

            var dic = new Dictionary<MinionSpawnType, float>
            {
                { MinionSpawnType.MINION_TYPE_MELEE, 19.8f + 0.2f * (int)(_game.GameTime / (90 * 1000)) },
                { MinionSpawnType.MINION_TYPE_CASTER, 16.8f + 0.2f * (int)(_game.GameTime / (90 * 1000)) },
                { MinionSpawnType.MINION_TYPE_CANNON, 40.0f + 0.5f * (int)(_game.GameTime / (90 * 1000)) },
                { MinionSpawnType.MINION_TYPE_SUPER, 40.0f + 1.0f * (int)(_game.GameTime / (180 * 1000)) }
            };

            if (!dic.ContainsKey(m.MinionSpawnType))
            {
                return 0.0f;
            }

            return dic[m.MinionSpawnType];
        }

        public float GetExperienceFor(IAttackableUnit u)
        {
            if (!(u is ILaneMinion m))
            {
                return 0.0f;
            }

            var dic = new Dictionary<MinionSpawnType, float>
            {
                { MinionSpawnType.MINION_TYPE_MELEE, 64.0f },
                { MinionSpawnType.MINION_TYPE_CASTER, 32.0f },
                { MinionSpawnType.MINION_TYPE_CANNON, 92.0f },
                { MinionSpawnType.MINION_TYPE_SUPER, 97.0f }
            };

            if (!dic.ContainsKey(m.MinionSpawnType))
            {
                return 0.0f;
            }

            return dic[m.MinionSpawnType];
        }

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

        public void SetMinionStats(ILaneMinion m)
        {
            // Same for all minions
            m.Stats.MoveSpeed.BaseValue = 325.0f;

            switch (m.MinionSpawnType)
            {
                case MinionSpawnType.MINION_TYPE_MELEE:
                    m.Stats.CurrentHealth = 475.0f + 20.0f * (int)(_game.GameTime / (180 * 1000));
                    m.Stats.HealthPoints.BaseValue = 475.0f + 20.0f * (int)(_game.GameTime / (180 * 1000));
                    m.Stats.AttackDamage.BaseValue = 12.0f + 1.0f * (int)(_game.GameTime / (180 * 1000));
                    m.Stats.Range.BaseValue = 180.0f;
                    m.Stats.AttackSpeedFlat = 1.250f;
                    m.IsMelee = true;
                    break;
                case MinionSpawnType.MINION_TYPE_CASTER:
                    m.Stats.CurrentHealth = 279.0f + 7.5f * (int)(_game.GameTime / (90 * 1000));
                    m.Stats.HealthPoints.BaseValue = 279.0f + 7.5f * (int)(_game.GameTime / (90 * 1000));
                    m.Stats.AttackDamage.BaseValue = 23.0f + 1.0f * (int)(_game.GameTime / (90 * 1000));
                    m.Stats.Range.BaseValue = 600.0f;
                    m.Stats.AttackSpeedFlat = 0.670f;
                    break;
                case MinionSpawnType.MINION_TYPE_CANNON:
                    m.Stats.CurrentHealth = 700.0f + 27.0f * (int)(_game.GameTime / (180 * 1000));
                    m.Stats.HealthPoints.BaseValue = 700.0f + 27.0f * (int)(_game.GameTime / (180 * 1000));
                    m.Stats.AttackDamage.BaseValue = 40.0f + 3.0f * (int)(_game.GameTime / (180 * 1000));
                    m.Stats.Range.BaseValue = 450.0f;
                    m.Stats.AttackSpeedFlat = 1.0f;
                    break;
                case MinionSpawnType.MINION_TYPE_SUPER:
                    m.Stats.CurrentHealth = 1500.0f + 200.0f * (int)(_game.GameTime / (180 * 1000));
                    m.Stats.HealthPoints.BaseValue = 1500.0f + 200.0f * (int)(_game.GameTime / (180 * 1000));
                    m.Stats.AttackDamage.BaseValue = 190.0f + 10.0f * (int)(_game.GameTime / (180 * 1000));
                    m.Stats.Range.BaseValue = 170.0f;
                    m.Stats.AttackSpeedFlat = 0.694f;
                    m.Stats.Armor.BaseValue = 30.0f;
                    m.Stats.MagicResist.BaseValue = -30.0f;
                    m.IsMelee = true;
                    break;
            }
        }

        public void SpawnMinion(List<MinionSpawnType> list, int minionNo, string barracksName, List<Vector2> waypoints)
        {
            if (list.Count <= minionNo)
            {
                return;
            }

            var team = GetMinionSpawnPosition(barracksName).Item1;
            var m = new LaneMinion(_game, list[minionNo], barracksName, waypoints, GetMinionModel(team, list[minionNo]), 0, team);
            _game.ObjectManager.AddObject(m);
        }

        public bool Spawn()
        {
            var barracks = new List<string>();
            foreach (var barrack in _mapData.SpawnBarracks)
            {
                barracks.Add(barrack.Value.Name);
            }

            var cannonMinionTimestamps = new List<Tuple<long, int>>
            {
                new Tuple<long, int>(0, 2),
                new Tuple<long, int>(20 * 60 * 1000, 1),
                new Tuple<long, int>(35 * 60 * 1000, 0)
            };

            var spawnToWaypoints = new Dictionary<string, Tuple<List<Vector2>, uint>>();
            foreach (var barrack in _mapData.SpawnBarracks)
            {
                if (!barrack.Value.Name.StartsWith("__P"))
                {
                    continue;
                }

                List<Vector2> waypoint = new List<Vector2>();
                TeamId opposed_team = barrack.Value.GetOpposingTeamID();
                LaneID lane = barrack.Value.GetSpawnBarrackLaneID();

                if (opposed_team == TeamId.TEAM_PURPLE)
                {
                    if (lane == LaneID.TOP)
                    {
                        waypoint = BlueTopWaypoints;
                    }
                    else if (lane == LaneID.MIDDLE)
                    {
                        waypoint = BlueMidWaypoints;
                    }
                    else if (lane == LaneID.BOTTOM)
                    {
                        waypoint = BlueBotWaypoints;
                    }
                }
                else if (opposed_team == TeamId.TEAM_BLUE) 
                {
                    if (lane == LaneID.TOP)
                    {
                        waypoint = RedTopWaypoints;
                    }
                    else if (lane == LaneID.MIDDLE)
                    {
                        waypoint = RedMidWaypoints;
                    }
                    else if (lane == LaneID.BOTTOM)
                    {
                        waypoint = RedBotWaypoints;
                    }
                }
                spawnToWaypoints.Add(barrack.Value.Name, Tuple.Create(waypoint, _inhibitors[opposed_team][lane][0].NetId));
            }
            var cannonMinionCap = 2;

            foreach (var timestamp in cannonMinionTimestamps)
            {
                if (_game.GameTime >= timestamp.Item1)
                {
                    cannonMinionCap = timestamp.Item2;
                }
            }

            foreach (var barracksName in barracks)
            {
                var waypoints = spawnToWaypoints[barracksName].Item1;
                var inhibitorId = spawnToWaypoints[barracksName].Item2;
                var inhibitor = _game.ObjectManager.GetInhibitorById(inhibitorId);
                var isInhibitorDead = inhibitor.InhibitorState == InhibitorState.DEAD && !inhibitor.RespawnAnnounced;

                var oppositeTeam = TeamId.TEAM_BLUE;
                if (inhibitor.Team == TeamId.TEAM_PURPLE)
                {
                    oppositeTeam = TeamId.TEAM_PURPLE;
                }

                var areAllInhibitorsDead = _game.ObjectManager.AllInhibitorsDestroyedFromTeam(oppositeTeam) && !inhibitor.RespawnAnnounced;

                var list = RegularMinionWave;
                if (_cannonMinionCount >= cannonMinionCap)
                {
                    list = CannonMinionWave;
                }

                if (isInhibitorDead)
                {
                    list = SuperMinionWave;
                }

                if (areAllInhibitorsDead)
                {
                    list = DoubleSuperMinionWave;
                }

                SpawnMinion(list, _minionNumber, barracksName, waypoints);
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

        public void HandleSurrender(int userId, IChampion who, bool vote)
        {
            if (_surrenders.ContainsKey(who.Team))
                _surrenders[who.Team].HandleSurrender(userId, who, vote);
        }
    }
}
