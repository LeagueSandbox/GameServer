using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;

namespace MapScripts.Map10
{
    public class CLASSIC : IMapScript
    {
        public IMapScriptMetadata MapScriptMetadata { get; set; } = new MapScriptMetadata
        {
            BaseGoldPerGoldTick = 0.95f,
            AIVars = new AIVars
            {
                StartingGold = 825.0f
            }
        };

        public virtual IGlobalData GlobalData { get; set; } = new GlobalData();
        public bool HasFirstBloodHappened { get; set; } = false;
        public long NextSpawnTime { get; set; } = 45 * 1000;
        public string LaneMinionAI { get; set; } = "LaneMinionAI";
        public Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> PlayerSpawnPoints { get; }

        //Minion models for this map
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
        public void Init(Dictionary<GameObjectTypes, List<MapObject>> mapObjects)
        {
            MapScriptMetadata.MinionSpawnEnabled = IsMinionSpawnEnabled();
            AddSurrender(1200000.0f, 300000.0f, 30.0f);

            LevelScriptObjects.LoadObjects(mapObjects);
            CreateLevelProps.CreateProps();
        }

        public void OnMatchStart()
        {
            LevelScriptObjects.OnMatchStart();
            NeutralMinionSpawn.InitializeCamps();
        }

        //This function gets executed every server tick
        public void Update(float diff)
        {
            LevelScriptObjects.OnUpdate(diff);
            NeutralMinionSpawn.OnUpdate(diff);

            float gameTime = GameTime();

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
            if (time >= 180.0f * 1000)
            {
                //The Altars have unlocked!
                NotifyWorldEvent(EventID.OnStartGameMessage4, 10);
                AllAnnouncementsAnnounced = true;
            }
            else if (time >= 150.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage2))
            {
                // The Altars will unlock in 30 seconds
                NotifyWorldEvent(EventID.OnStartGameMessage2, 10);
                AnnouncedEvents.Add(EventID.OnStartGameMessage2);

            }
            else if (time >= 75.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage3))
            {
                // Minions have Spawned
                NotifyWorldEvent(EventID.OnStartGameMessage3, 10);
                NotifyWorldEvent(EventID.OnNexusCrystalStart, 0);
                AnnouncedEvents.Add(EventID.OnStartGameMessage3);
            }
            else if (time >= 30.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage1))
            {
                // Welcome to the Twisted Tree Line!
                NotifyWorldEvent(EventID.OnStartGameMessage1, 10);
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
            foreach (var team in LevelScriptObjects.SpawnBarracks.Keys)
            {
                foreach (var barrack in LevelScriptObjects.SpawnBarracks[team].Values)
                {
                    TeamId opposed_team = barrack.GetOpposingTeamID();
                    LaneID lane = barrack.GetSpawnBarrackLaneID();
                    MapObject opposedBarrack = LevelScriptObjects.SpawnBarracks[opposed_team][lane];
                    IInhibitor inhibitor = LevelScriptObjects.InhibitorList[opposed_team][lane];
                    Vector2 position = new Vector2(barrack.CentralPoint.X, barrack.CentralPoint.Z);
                    bool isInhibitorDead = inhibitor.InhibitorState == InhibitorState.DEAD;
                    Tuple<int, List<MinionSpawnType>> spawnWave = MinionWaveToSpawn(GameTime(), _cannonMinionCount, isInhibitorDead, LevelScriptObjects.AllInhibitorsAreDead[opposed_team]);
                    cannonMinionCap = spawnWave.Item1;

                    List<Vector2> waypoint = new List<Vector2>(LevelScriptObjects.MinionPaths[lane]);
                    if (team == TeamId.TEAM_PURPLE)
                    {
                        waypoint.Reverse();
                    }

                    waypoint.Add(new Vector2(opposedBarrack.CentralPoint.X, opposedBarrack.CentralPoint.Z));
                    CreateLaneMinion(spawnWave.Item2, position, team, _minionNumber, barrack.Name, waypoint);
                }
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
