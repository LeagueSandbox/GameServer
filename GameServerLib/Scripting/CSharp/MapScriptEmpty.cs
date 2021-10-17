using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Force.Crc32;
using GameServerCore;
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
    public class MapScriptEmpty : IMapScript
    {
        public bool HasTopLane { get; set; } = false;
        public bool HasMidLane { get; set; } = false;
        public bool HasBotLane { get; set; } = false;

        private static readonly List<Vector2> BlueTopWaypoints = new List<Vector2>
        {
        };
        private static readonly List<Vector2> BlueBotWaypoints = new List<Vector2>
        {
        };
        private static readonly List<Vector2> BlueMidWaypoints = new List<Vector2>
        {
        };
        private static readonly List<Vector2> RedTopWaypoints = new List<Vector2>
        {
        };
        private static readonly List<Vector2> RedBotWaypoints = new List<Vector2>
        {
        };
        private static readonly List<Vector2> RedMidWaypoints = new List<Vector2>
        {
        };

        private static readonly List<MinionSpawnType> RegularMinionWave = new List<MinionSpawnType>
        {
        };
        private static readonly List<MinionSpawnType> CannonMinionWave = new List<MinionSpawnType>
        {
        };
        private static readonly List<MinionSpawnType> SuperMinionWave = new List<MinionSpawnType>
        {
        };
        private static readonly List<MinionSpawnType> DoubleSuperMinionWave = new List<MinionSpawnType>
        {
        };

        private Game _game;
        private MapData _mapData;
        private IMap _map;
        private int _cannonMinionCount;
        private int _minionNumber;
        private readonly long _firstSpawnTime = 90 * 1000;
        private long _nextSpawnTime = 90 * 1000;
        private readonly long _spawnInterval = 30 * 1000;
        private readonly Dictionary<TeamId, SurrenderHandler> _surrenders = new Dictionary<TeamId, SurrenderHandler>();
        private Dictionary<TeamId, Fountain> _fountains = new Dictionary<TeamId, Fountain>();
        public Dictionary<TurretType, int[]> TurretItems { get; set; } = new Dictionary<TurretType, int[]>
        {
            { TurretType.OUTER_TURRET, new[] { 1500, 1501, 1502, 1503 } },
            { TurretType.INNER_TURRET, new[] { 1500, 1501, 1502, 1503, 1504 } },
            { TurretType.INHIBITOR_TURRET, new[] { 1501, 1502, 1503, 1505 } },
            { TurretType.NEXUS_TURRET, new[] { 1501, 1502, 1503, 1505 } }
        };
        public Dictionary<TeamId, Dictionary<TurretType, string>> TowerModels { get; set; } = new Dictionary<TeamId, Dictionary<TurretType, string>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "TurretShrine" },
                {TurretType.NEXUS_TURRET, "TurretAngel" },
                {TurretType.INHIBITOR_TURRET, "TurretDragon" },
                {TurretType.INNER_TURRET, "TurretNormal2" },
                {TurretType.OUTER_TURRET, "TurretNormal" },
            } },
            {TeamId.TEAM_PURPLE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "TurretShrine" },
                {TurretType.NEXUS_TURRET, "TurretNormal" },
                {TurretType.INHIBITOR_TURRET, "TurretGiant" },
                {TurretType.INNER_TURRET, "TurretWorm2" },
                {TurretType.OUTER_TURRET, "TurretWorm" },
            } }
        };
        public Dictionary<TeamId, Dictionary<MinionSpawnType, string>> MinionModels { get; set; } = new Dictionary<TeamId, Dictionary<MinionSpawnType, string>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<MinionSpawnType, string>{
                {MinionSpawnType.MINION_TYPE_MELEE, "Blue_Minion_Basic"},
                {MinionSpawnType.MINION_TYPE_CASTER, "Blue_Minion_Wizard"},
                {MinionSpawnType.MINION_TYPE_CANNON, "Blue_Minion_MechCannon"},
                {MinionSpawnType.MINION_TYPE_SUPER, "Blue_Minion_MechMelee"}
            }},
            {TeamId.TEAM_PURPLE, new Dictionary<MinionSpawnType, string>{
                {MinionSpawnType.MINION_TYPE_MELEE, "Red_Minion_Basic"},
                {MinionSpawnType.MINION_TYPE_CASTER, "Red_Minion_Wizard"},
                {MinionSpawnType.MINION_TYPE_CANNON, "Red_Minion_MechCannon"},
                {MinionSpawnType.MINION_TYPE_SUPER, "Red_Minion_MechMelee"}
            }}
        };

        public float GoldPerSecond { get; set; } = 1.9f;
        public float StartingGold { get; set; } = 475.0f;
        public bool HasFirstBloodHappened { get; set; } = false;
        public bool IsKillGoldRewardReductionActive { get; set; } = true;
        public int BluePillId { get; set; } = 2001;
        public long FirstGoldTime { get; set; } = 90 * 1000;
        public bool SpawnEnabled { get; set; }

        public MapScriptEmpty(Game game)
        {
            _game = game;
            _mapData = game.Config.MapData;

            _surrenders.Add(TeamId.TEAM_BLUE, new SurrenderHandler(_game, TeamId.TEAM_BLUE, 1200000.0f, 300000.0f, 30.0f));
            _surrenders.Add(TeamId.TEAM_PURPLE, new SurrenderHandler(_game, TeamId.TEAM_PURPLE, 1200000.0f, 300000.0f, 30.0f));

            SpawnEnabled = _game.Config.MinionSpawnsEnabled;
        }
        public void AddFountain(TeamId team, Vector2 position)
        {
            _fountains.Add(team, new Fountain(_game, team, position, 1000));
        }

        public int[] GetTurretItems(TurretType type)
        {
            Dictionary<TurretType, int[]> TurretItems = new Dictionary<TurretType, int[]>
            {
            };

            if (!TurretItems.ContainsKey(type))
            {
                return null;
            }

            return TurretItems[type];
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

            switch (trueIndex)
            {
                case 1:
                case 4:
                case 5:
                    returnType = TurretType.INHIBITOR_TURRET;
                    break;
                case 2:
                    returnType = TurretType.INNER_TURRET;
                    break;
                case 3:
                    returnType = TurretType.OUTER_TURRET;
                    break;
            }

            return returnType;
        }

        public string GetTowerModel(TurretType type, TeamId teamId)
        {
            string towerModel = "";
            if (teamId == TeamId.TEAM_BLUE)
            {
                switch (type)
                {
                    case TurretType.FOUNTAIN_TURRET:
                        towerModel = "";
                        break;

                    case TurretType.NEXUS_TURRET:
                        towerModel = "";
                        break;

                    case TurretType.INHIBITOR_TURRET:
                        towerModel = "";
                        break;

                    case TurretType.INNER_TURRET:
                        towerModel = "";
                        break;

                    case TurretType.OUTER_TURRET:
                        towerModel = "";
                        break;

                }
            }
            else
            {
                switch (type)
                {
                    case TurretType.FOUNTAIN_TURRET:
                        towerModel = "";
                        break;

                    case TurretType.NEXUS_TURRET:
                        towerModel = "";
                        break;

                    case TurretType.INHIBITOR_TURRET:
                        towerModel = "";
                        break;

                    case TurretType.INNER_TURRET:
                        towerModel = "";
                        break;

                    case TurretType.OUTER_TURRET:
                        towerModel = "";
                        break;

                }
            }
            return towerModel;
        }
        public void ChangeTowerOnMapList(string towerName, TeamId team, LaneID currentLaneId, LaneID desiredLaneID)
        {
            var tower = _map._turrets[team][currentLaneId].Find(x => x.Name == towerName);
            tower.SetLaneID(desiredLaneID);
            _map._turrets[team][currentLaneId].Remove(tower);
            _map._turrets[team][desiredLaneID].Add(tower);
        }
        public void Init(IMap map)
        {
            _map = map;
        }

        public void Update(float diff)
        {
            foreach (var fountain in _fountains.Values)
            {
                fountain.Update(diff);
            }

            foreach (var surrender in _surrenders.Values)
                surrender.Update(diff);
        }

        public string GetMinionModel(TeamId team, MinionSpawnType type)
        {
            return "";
        }

        public float GetGoldFor(IAttackableUnit u)
        {
            return 0;
        }

        public float GetExperienceFor(IAttackableUnit u)
        {
            return 0;
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
        }

        public void SpawnMinion(List<MinionSpawnType> list, int minionNo, string barracksName, List<Vector2> waypoints)
        {
        }

        public bool Spawn()
        {
            return false;
        }

        public void HandleSurrender(int userId, IChampion who, bool vote)
        {
            if (_surrenders.ContainsKey(who.Team))
                _surrenders[who.Team].HandleSurrender(userId, who, vote);
        }
    }
}
