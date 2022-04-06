using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;

namespace MapScripts.Map11
{
    public class CLASSIC : IMapScript
    {
        public virtual IMapScriptMetadata MapScriptMetadata { get; set; } = new MapScriptMetadata();
        public virtual IGlobalData GlobalData { get; set; } = new GlobalData();
        public bool HasFirstBloodHappened { get; set; } = false;
        public long NextSpawnTime { get; set; } = 90 * 1000;
        public string LaneMinionAI { get; set; } = "LaneMinionAI";
        public Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> PlayerSpawnPoints { get; }
        public Dictionary<TeamId, Dictionary<MinionSpawnType, string>> MinionModels { get; set; } = new Dictionary<TeamId, Dictionary<MinionSpawnType, string>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<MinionSpawnType, string>{
                {MinionSpawnType.MINION_TYPE_MELEE, "SRU_OrderMinionMelee"},
                {MinionSpawnType.MINION_TYPE_CASTER, "SRU_OrderMinionRanged"},
                {MinionSpawnType.MINION_TYPE_CANNON, "SRU_OrderMinionSiege"},
                {MinionSpawnType.MINION_TYPE_SUPER, "SRU_OrderMinionSuper"}
            }},
            {TeamId.TEAM_PURPLE, new Dictionary<MinionSpawnType, string>{
                {MinionSpawnType.MINION_TYPE_MELEE, "SRU_ChaosMinionMelee"},
                {MinionSpawnType.MINION_TYPE_CASTER, "SRU_ChaosMinionRanged"},
                {MinionSpawnType.MINION_TYPE_CANNON, "SRU_ChaosMinionSiege"},
                {MinionSpawnType.MINION_TYPE_SUPER, "SRU_ChaosMinionSuper"}
            }}
        };

        //List of every path minions will take, separated by lane (These values are for team blue, team red will just reverse this table
        public Dictionary<LaneID, List<Vector2>> MinionPaths { get; set; } = new Dictionary<LaneID, List<Vector2>>
        {
            //Pathing coordinates for Top lane
            {LaneID.TOP, new List<Vector2> {
                new Vector2(1341f, 2274f),
                new Vector2(1544f, 3567f),
                new Vector2(1410f, 4262f),
                new Vector2(1232f, 6666f),
                new Vector2(1295f, 10400f),
                new Vector2(1371f, 11375f),
                new Vector2(2206f, 12601f),
                new Vector2(3239f, 13402f),
                new Vector2(4300f, 13575f),
                new Vector2(7960f, 13656f),
                new Vector2(10490f, 13900f),
                new Vector2(11258f, 14000f),
                new Vector2(12707f, 13542f)
            }},

            //Pathing coordinates for Mid lane
            {LaneID.MIDDLE, new List<Vector2> {
                new Vector2(2126f, 2172f),
                new Vector2(2850f, 2926f),
                new Vector2(3318.8f, 2859f),
                new Vector2(3914f, 3535f),
                new Vector2(4839f, 5004f),
                new Vector2(7450f, 7450f),
                new Vector2(10012f, 9926f),
                new Vector2(11385f, 10950f),
                new Vector2(11864f, 11420f),
                new Vector2(11902f, 11960f),
                new Vector2(12723f, 12773f)
            }},

            //Pathing coordinates for Bot lane
            {LaneID.BOTTOM, new List<Vector2> {
                new Vector2(2271f, 1352f),
                new Vector2(2943f, 1251f),
                new Vector2(3453f, 1569f),
                new Vector2(4302f, 1542f),
                new Vector2(4764f, 1219f),
                new Vector2(6890f, 1200f),
                new Vector2(10508f, 1311f),
                new Vector2(11262f, 1424f),
                new Vector2(11919f, 1815f),
                new Vector2(12575f, 2450f),
                new Vector2(13157f, 3060f),
                new Vector2(13536f, 3831f),
                new Vector2(13571f, 4500f),
                new Vector2(13653f, 8236f),
                new Vector2(13626f, 10040f),
                new Vector2(13336f, 10542f),
                new Vector2(13300f, 11314f),
                new Vector2(13606f, 11720f),
                new Vector2(13606f, 12525f)
            }}
        };

        //List of every wave type
        public Dictionary<string, List<MinionSpawnType>> MinionWaveTypes = new Dictionary<string, List<MinionSpawnType>>
        { {"RegularMinionWave", new List<MinionSpawnType>
        {
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER }
        },
        {"CannonMinionWave", new List<MinionSpawnType>{
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CANNON,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER }
        },
        {"SuperMinionWave", new List<MinionSpawnType>{
            MinionSpawnType.MINION_TYPE_SUPER,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER }
        },
        {"DoubleSuperMinionWave", new List<MinionSpawnType>{
            MinionSpawnType.MINION_TYPE_SUPER,
            MinionSpawnType.MINION_TYPE_SUPER,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_MELEE,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER,
            MinionSpawnType.MINION_TYPE_CASTER }
        }};

        //This function is executed in-between Loading the map structures and applying the structure protections. Is the first thing on this script to be executed
        public virtual void Init(Dictionary<GameObjectTypes, List<MapObject>> mapObjects)
        {
            MapScriptMetadata.MinionSpawnEnabled = IsMinionSpawnEnabled();
            AddSurrender(1200000.0f, 300000.0f, 30.0f);

            LevelScriptObjects.LoadObjects(mapObjects);
            CreateLevelProps.CreateProps();
        }

        public virtual void OnMatchStart()
        {
            LevelScriptObjects.OnMatchStart();
            NeutralMinionSpawn.InitializeCamps();
        }

        public void Update(float diff)
        {
            LevelScriptObjects.OnUpdate(diff);
            NeutralMinionSpawn.OnUpdate(diff);

            var gameTime = GameTime();

            if (MapScriptMetadata.MinionSpawnEnabled)
            {
                if (_minionNumber > 0)
                {
                    // Spawn new Minion every 0.8s
                    if (gameTime >= NextSpawnTime + _minionNumber * 8 * 100)
                    {
                        if (SetUpLaneMinion())
                        {
                            _minionNumber = 0;
                            NextSpawnTime = (long)gameTime + MapScriptMetadata.SpawnInterval;
                        }
                        else
                        {
                            _minionNumber++;
                        }
                    }
                }
                else if (gameTime >= NextSpawnTime)
                {
                    SetUpLaneMinion();
                    _minionNumber++;
                }
            }

            if (!AllAnnouncementsAnnounced)
            {
                CheckInitialMapAnnouncements(gameTime);
            }
        }

        public void SpawnAllCamps()
        {
            NeutralMinionSpawn.ForceCampSpawn();
        }

        public Vector2 GetFountainPosition(TeamId team)
        {
            return LevelScriptObjects.FountainList[team].Position;
        }

        bool AllAnnouncementsAnnounced = false;
        List<EventID> AnnouncedEvents = new List<EventID>();
        public void CheckInitialMapAnnouncements(float time)
        {
            if (time >= 90.0f * 1000)
            {
                // Minions have spawned
                NotifyWorldEvent(EventID.OnMinionsSpawn, 0);
                NotifyWorldEvent(EventID.OnNexusCrystalStart, 0);
                AllAnnouncementsAnnounced = true;
            }
            else if (time >= 60.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage2))
            {
                // 30 seconds until minions spawn
                NotifyWorldEvent(EventID.OnStartGameMessage2, 11);
                AnnouncedEvents.Add(EventID.OnStartGameMessage2);
            }
            else if (time >= 30.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage1))
            {
                // Welcome to Summoners Rift
                NotifyWorldEvent(EventID.OnStartGameMessage1, 11);
                AnnouncedEvents.Add(EventID.OnStartGameMessage1);
            }
        }

        //Here you setup the conditions of which wave will be spawned
        public Tuple<int, List<MinionSpawnType>> MinionWaveToSpawn(float gameTime, int cannonMinionCount, bool isInhibitorDead, bool areAllInhibitorsDead)
        {
            var cannonMinionTimestamps = new List<Tuple<long, int>>
            {
                new Tuple<long, int>(0, 2),
                new Tuple<long, int>(20 * 60 * 1000, 1),
                new Tuple<long, int>(35 * 60 * 1000, 0)
            };
            var cannonMinionCap = 2;

            foreach (var timestamp in cannonMinionTimestamps)
            {
                if (gameTime >= timestamp.Item1)
                {
                    cannonMinionCap = timestamp.Item2;
                }
            }
            var list = "RegularMinionWave";
            if (cannonMinionCount >= cannonMinionCap)
            {
                list = "CannonMinionWave";
            }

            if (isInhibitorDead)
            {
                list = "SuperMinionWave";
            }

            if (areAllInhibitorsDead)
            {
                list = "DoubleSuperMinionWave";
            }
            return new Tuple<int, List<MinionSpawnType>>(cannonMinionCap, MinionWaveTypes[list]);
        }

        public int _minionNumber;
        public int _cannonMinionCount;
        public bool SetUpLaneMinion()
        {
            int cannonMinionCap = 2;
            foreach (var barrack in LevelScriptObjects.SpawnBarracks)
            {
                TeamId opposed_team = barrack.Value.GetOpposingTeamID();
                TeamId barrackTeam = barrack.Value.GetTeamID();
                LaneID lane = barrack.Value.GetSpawnBarrackLaneID();
                IInhibitor inhibitor = LevelScriptObjects.InhibitorList[opposed_team][lane];
                Vector2 position = new Vector2(barrack.Value.CentralPoint.X, barrack.Value.CentralPoint.Z);
                bool isInhibitorDead = inhibitor.InhibitorState == InhibitorState.DEAD;
                Tuple<int, List<MinionSpawnType>> spawnWave = MinionWaveToSpawn(GameTime(), _cannonMinionCount, isInhibitorDead, LevelScriptObjects.AllInhibitorsAreDead[opposed_team]);
                cannonMinionCap = spawnWave.Item1;

                List<Vector2> waypoint = new List<Vector2>(MinionPaths[lane]);
                if (barrackTeam == TeamId.TEAM_PURPLE)
                {
                    waypoint.Reverse();
                }

                CreateLaneMinion(spawnWave.Item2, position, barrackTeam, _minionNumber, barrack.Value.Name, waypoint);
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
    }
}
