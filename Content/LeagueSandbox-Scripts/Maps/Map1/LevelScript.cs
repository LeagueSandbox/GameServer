using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.Content;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.Scripting.CSharp;
using LeagueSandbox.GameServer.GameObjects.Stats;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;

namespace MapScripts.Map1
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

        //List of every path minions will take, separated by team and lane
        public Dictionary<LaneID, List<Vector2>> MinionPaths { get; set; } = new Dictionary<LaneID, List<Vector2>>
        {
            //Pathing coordinates for Top lane
            {LaneID.TOP, new List<Vector2> {
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
                new Vector2(12511.0f, 12776.0f) }
            },

            //Pathing coordinates for Mid lane
            {LaneID.MIDDLE, new List<Vector2> {
                new Vector2(1418.0f, 1686.0f),
                new Vector2(2997.0f, 2781.0f),
                new Vector2(4472.0f, 4727.0f),
                new Vector2(8375.0f, 8366.0f),
                new Vector2(10948.0f, 10821.0f),
                new Vector2(12511.0f, 12776.0f) }
            },

            //Pathing coordinates for Bot lane
            {LaneID.BOTTOM, new List<Vector2> {
                new Vector2(1487.0f, 1302.0f),
                new Vector2(3789.0f, 1346.0f),
                new Vector2(6430.0f, 1005.0f),
                new Vector2(10995.0f, 1234.0f),
                new Vector2(12841.0f, 3051.0f),
                new Vector2(13148.0f, 4202.0f),
                new Vector2(13249.0f, 7884.0f),
                new Vector2(12886.0f, 10356.0f),
                new Vector2(12511.0f, 12776.0f) }
            }
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

        //These minion modifiers will remain unused for the moment, untill i pull the spawning systems to MapScripts
        Dictionary<MinionSpawnType, IStatsModifier> MinionModifiers = new Dictionary<MinionSpawnType, IStatsModifier>
        {
            { MinionSpawnType.MINION_TYPE_MELEE, new StatsModifier() },
            { MinionSpawnType.MINION_TYPE_CASTER, new StatsModifier() },
            { MinionSpawnType.MINION_TYPE_CANNON, new StatsModifier() },
            { MinionSpawnType.MINION_TYPE_SUPER, new StatsModifier() },
        };

        //This function is executed in-between Loading the map structures and applying the structure protections. Is the first thing on this script to be executed
        public virtual void Init(Dictionary<GameObjectTypes, List<MapObject>> mapObjects)
        {
            MapScriptMetadata.MinionSpawnEnabled = IsMinionSpawnEnabled();
            AddSurrender(1200000.0f, 300000.0f, 30.0f);

            MinionModifiers[MinionSpawnType.MINION_TYPE_MELEE].GoldGivenOnDeath.FlatBonus = 0.5f;
            MinionModifiers[MinionSpawnType.MINION_TYPE_MELEE].HealthPoints.FlatBonus = 20.0f;
            MinionModifiers[MinionSpawnType.MINION_TYPE_MELEE].AttackDamage.FlatBonus = 1.0f;
            MinionModifiers[MinionSpawnType.MINION_TYPE_MELEE].Armor.FlatBonus = 3.0f;
            MinionModifiers[MinionSpawnType.MINION_TYPE_MELEE].MagicResist.FlatBonus = 1.25f;

            MinionModifiers[MinionSpawnType.MINION_TYPE_CASTER].GoldGivenOnDeath.FlatBonus = 0.2f;
            MinionModifiers[MinionSpawnType.MINION_TYPE_CASTER].HealthPoints.FlatBonus = 7.5f;
            MinionModifiers[MinionSpawnType.MINION_TYPE_CASTER].AttackDamage.FlatBonus = 1.0f;
            MinionModifiers[MinionSpawnType.MINION_TYPE_CASTER].Armor.FlatBonus = 0.625f;
            MinionModifiers[MinionSpawnType.MINION_TYPE_CASTER].MagicResist.FlatBonus = 1.0f;

            MinionModifiers[MinionSpawnType.MINION_TYPE_CANNON].GoldGivenOnDeath.FlatBonus = 1f;
            MinionModifiers[MinionSpawnType.MINION_TYPE_CANNON].HealthPoints.FlatBonus = 27.0f;
            MinionModifiers[MinionSpawnType.MINION_TYPE_CANNON].AttackDamage.FlatBonus = 3.0f;
            MinionModifiers[MinionSpawnType.MINION_TYPE_CANNON].Armor.FlatBonus = 3.0f;
            MinionModifiers[MinionSpawnType.MINION_TYPE_CANNON].MagicResist.FlatBonus = 3.0f;

            MinionModifiers[MinionSpawnType.MINION_TYPE_SUPER].GoldGivenOnDeath.FlatBonus = 1.0f;
            MinionModifiers[MinionSpawnType.MINION_TYPE_SUPER].HealthPoints.FlatBonus = 200.0f;
            MinionModifiers[MinionSpawnType.MINION_TYPE_SUPER].AttackDamage.FlatBonus = 10.0f;

            CreateLevelProps.CreateProps();
            LevelScriptObjects.LoadBuildings(mapObjects);
        }

        public virtual void OnMatchStart()
        {
            NeutralMinionSpawn.InitializeCamps();
            LevelScriptObjects.OnMatchStart();
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
                NotifyWorldEvent(EventID.OnStartGameMessage2, 1);
                AnnouncedEvents.Add(EventID.OnStartGameMessage2);
            }
            else if (time >= 30.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage1))
            {
                // Welcome to Summoners Rift
                NotifyWorldEvent(EventID.OnStartGameMessage1, 1);
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
