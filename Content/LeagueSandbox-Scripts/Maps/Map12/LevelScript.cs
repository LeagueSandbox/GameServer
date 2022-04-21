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
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace MapScripts.Map12
{
    public class ARAM : IMapScript
    {
        public IMapScriptMetadata MapScriptMetadata { get; set; } = new MapScriptMetadata
        {
            BaseGoldPerGoldTick = 1.7f,
            RecallSpellItemId = 2007,
            InitialLevel = 3,
            AIVars = new AIVars
            {
                GoldRadius = 0.0f,
                StartingGold = 1375.0f
            }
        };

        public virtual IGlobalData GlobalData { get; set; } = new GlobalData();
        public bool HasFirstBloodHappened { get; set; } = false;
        public long NextSpawnTime { get; set; } = 60 * 1000;
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

        public Dictionary<LaneID, List<Vector2>> MinionPaths { get; set; }

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
        }};

        public Dictionary<int, ILevelProp> Poros = new Dictionary<int, ILevelProp>();
        public Dictionary<int, ILevelProp> LongChains = new Dictionary<int, ILevelProp>();
        public Dictionary<int, ILevelProp> Chains = new Dictionary<int, ILevelProp>();
        public void Init(Dictionary<GameObjectTypes, List<MapObject>> mapObjects)
        {
            MapScriptMetadata.MinionSpawnEnabled = IsMinionSpawnEnabled();

            AddSurrender(1200000.0f, 300000.0f, 30.0f);

            CreateLevelProps.CreateProps(this);
            LevelScriptObjects.LoadObjects(mapObjects);
        }

        public void OnMatchStart()
        {
            foreach (var champion in GetAllPlayers())
            {
                AddBuff("HowlingAbyssAura", 25000, 1, null, champion, null);
            }

            NotifyPropAnimation(Poros[0], "idle1_beam", (AnimationFlags)1, 0.0f, false);
            NotifyPropAnimation(Poros[2], "idle1_beam", (AnimationFlags)1, 0.0f, false);

            foreach (var key in Poros.Keys)
            {
                if (key == 0 || key == 2)
                {
                    continue;
                }
                NotifyPropAnimation(Poros[key], "idle1_prop", (AnimationFlags)1, 0.0f, false);
            }

            NotifyPropAnimation(Poros[0], "disappear", (AnimationFlags)2, 0.0f, false);
            NotifyPropAnimation(Poros[1], "disappear", (AnimationFlags)2, 0.0f, false);

            NotifyPropAnimation(LongChains[0], "W_Wind_Strong", (AnimationFlags)1, 0.0f, false);
            NotifyPropAnimation(LongChains[3], "N_Wind_Strong", (AnimationFlags)1, 0.0f, false);
            NotifyPropAnimation(LongChains[2], "S_Wind_Strong", (AnimationFlags)1, 0.0f, false);
            NotifyPropAnimation(LongChains[2], "E_Wind_Strong", (AnimationFlags)1, 0.0f, false);


            NotifyPropAnimation(Chains[4], "wind_low", (AnimationFlags)1, 0.0f, false);
            NotifyPropAnimation(Chains[2], "wind_low", (AnimationFlags)1, 0.0f, false);

            LevelScriptObjects.OnMatchStart();
            NeutralMinionSpawn.InitializeJungle();
        }

        //This function gets executed every server tick
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
                CheckMapInitialAnnouncements(gameTime);
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
        public void CheckMapInitialAnnouncements(float time)
        {
            if (time >= 60.0f * 1000)
            {
                // Minions have spawned
                NotifyWorldEvent(EventID.OnMinionsSpawn, 0);
                NotifyWorldEvent(EventID.OnNexusCrystalStart, 0);
                AllAnnouncementsAnnounced = true;
            }
            else if (time >= 30.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage1))
            {
                // Welcome to the Howling Abyss
                NotifyWorldEvent(EventID.OnStartGameMessage1, 12);
                AnnouncedEvents.Add(EventID.OnStartGameMessage1);
            }
        }

        //Here you setup the conditions of which wave will be spawned
        public Tuple<int, List<MinionSpawnType>> MinionWaveToSpawn(float gameTime, int cannonMinionCount, bool isInhibitorDead)
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
                    IInhibitor inhibitor = LevelScriptObjects.InhibitorList[opposed_team];
                    Vector2 position = new Vector2(barrack.CentralPoint.X, barrack.CentralPoint.Z);
                    bool isInhibitorDead = inhibitor.InhibitorState == InhibitorState.DEAD;
                    Tuple<int, List<MinionSpawnType>> spawnWave = MinionWaveToSpawn(GameTime(), _cannonMinionCount, isInhibitorDead);
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
