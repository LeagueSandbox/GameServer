using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Maps;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.Scripting.CSharp;
using GameServerCore.Scripting.CSharp;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using static GameServerLib.API.APIMapFunctionManager;

namespace MapScripts.Map8
{
    public class ODIN : IMapScript
    {
        public IMapScriptMetadata MapScriptMetadata { get; set; } = new MapScriptMetadata
        {
            MinionSpawnEnabled = false,
            StartingGold = 1375.0f,
            OverrideSpawnPoints = true,
            RecallSpellItemId = 2005,
            GoldPerSecond = 5.6f,
            FirstGoldTime = 90.0f * 1000,
            InitialLevel = 3
        };

        private IMapScriptHandler _map;
        public virtual IGlobalData GlobalData { get; set; } = new GlobalData();
        public bool HasFirstBloodHappened { get; set; } = false;
        public long NextSpawnTime { get; set; } = 90 * 1000;
        public string LaneMinionAI { get; set; } = "LaneMinionAI";
        public string LaneTurretAI { get; set; } = "TurretAI";

        //Values i got the values for 5 players from replay packets, the value for 1 player is just a guess of mine by using !coords command in-game
        public Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> PlayerSpawnPoints { get; } = new Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<int, Dictionary<int, Vector2>>{
                { 5, new Dictionary<int, Vector2>{
                    { 1, new Vector2(687.99036f, 4281.2314f) },
                    { 2, new Vector2(687.99036f, 4061.2314f) },
                    { 3, new Vector2(478.79034f,3993.2314f) },
                    { 4, new Vector2(349.39032f,4171.2314f) },
                    { 5, new Vector2(438.79034f,4349.2314f) }
                }},
                {1, new Dictionary<int, Vector2>{
                    { 1, new Vector2(580f, 4124f) }
                }}
            }},

            {TeamId.TEAM_PURPLE, new Dictionary<int, Dictionary<int, Vector2>>
                { { 5, new Dictionary<int, Vector2>{
                    { 1, new Vector2(13468.365f,4281.2324f) },
                    { 2, new Vector2(13468.365f,4061.2324f) },
                    { 3, new Vector2(13259.165f,3993.2324f) },
                    { 4, new Vector2(13129.765f,4171.2324f) },
                    { 5, new Vector2(13219.165f,4349.2324f) }
                }},
                {1, new Dictionary<int, Vector2>{
                    { 1, new Vector2(13310f, 4124f) }
                }}
            }},

        };

        //Tower type enumeration might vary slightly from map to map, so we set that up here
        public TurretType GetTurretType(int trueIndex, LaneID lane, TeamId teamId)
        {
            return TurretType.NEXUS_TURRET;
        }

        //Nexus models
        //Nexus and Inhibitor model changes dont seem to take effect in-game, has to be investigated.
        public Dictionary<TeamId, string> NexusModels { get; set; } = new Dictionary<TeamId, string>
        {
            {TeamId.TEAM_BLUE, "OrderNexus" },
            {TeamId.TEAM_PURPLE, "ChaosNexus" }
        };
        //Inhib models
        public Dictionary<TeamId, string> InhibitorModels { get; set; } = new Dictionary<TeamId, string>
        {
            {TeamId.TEAM_BLUE, "OrderInhibitor" },
            {TeamId.TEAM_PURPLE, "ChaosInhibitor" }
        };
        //Tower Models
        public Dictionary<TeamId, Dictionary<TurretType, string>> TowerModels { get; set; } = new Dictionary<TeamId, Dictionary<TurretType, string>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "OdinOrderTurretShrine" },
            } },
            {TeamId.TEAM_PURPLE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "OdinChaosTurretShrine" },
            } }
        };

        //Turret Items
        public Dictionary<TurretType, int[]> TurretItems { get; set; } = new Dictionary<TurretType, int[]>
        {
            { TurretType.OUTER_TURRET, new[] { 1500, 1501, 1502, 1503 } },
            { TurretType.INNER_TURRET, new[] { 1500, 1501, 1502, 1503, 1504 } },
            { TurretType.INHIBITOR_TURRET, new[] { 1501, 1502, 1503, 1505 } },
            { TurretType.NEXUS_TURRET, new[] { 1501, 1502, 1503, 1505 } }
        };

        //List of every path minions will take, separated by team and lane
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
        }
        };

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

        public Dictionary<TeamId, ILevelProp> TeamStairs = new Dictionary<TeamId, ILevelProp>();
        public Dictionary<TeamId, ILevelProp> Nexus = new Dictionary<TeamId, ILevelProp>();
        public Dictionary<int, ILevelProp> SwainBeams = new Dictionary<int, ILevelProp>();
        //This function is executed in-between Loading the map structures and applying the structure protections. Is the first thing on this script to be executed
        public void Init(IMapScriptHandler map)
        {
            _map = map;

            //TODO: Implement Dynamic Minion spawn mechanics for Map8
            //SpawnEnabled = map.IsMinionSpawnEnabled();
            AddSurrender(1200000.0f, 300000.0f, 30.0f);

            CreateLevelProps.CreateProps(this);
        }

        public void OnMatchStart()
        {

            AddParticleTarget(Nexus[TeamId.TEAM_BLUE], Nexus[TeamId.TEAM_BLUE], "Odin_Crystal_blue", Nexus[TeamId.TEAM_BLUE], 25000);
            AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], Nexus[TeamId.TEAM_PURPLE], "Odin_Crystal_purple", Nexus[TeamId.TEAM_PURPLE], 25000);

            AddParticle(null, null, "Odin_Forcefield_blue", new Vector2(580f, 4124f), 80.0f);
            AddParticle(null, null, "Odin_Forcefield_purple", new Vector2(13310f, 4124f), 80.0f);

            foreach (var stair in TeamStairs.Values)
            {
                NotifyPropAnimation(stair, "Open", (AnimationFlags)1, 0.0f, false);
            }
            NotifyPropAnimation(SwainBeams[1], "PeckA", (AnimationFlags)1, 0.0f, false);
            NotifyPropAnimation(SwainBeams[2], "PeckB", (AnimationFlags)1, 0.0f, false);
            NotifyPropAnimation(SwainBeams[3], "PeckC", (AnimationFlags)1, 0.0f, false);

            foreach (var champion in GetAllPlayers())
            {
                AddBuff("OdinPlayerBuff", 25000, 1, null, champion, null);
            }

            NeutralMinionSpawn.InitializeNeutrals(_map);
        }

        public void Update(float diff)
        {
            NeutralMinionSpawn.OnUpdate(diff);

            var gameTime = GameTime();

            if (!NotifiedAllInitialAnimations)
            {
                InitialBaseAnimations(gameTime);
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

        bool AllAnnouncementsAnnounced = false;
        List<EventID> AnnouncedEvents = new List<EventID>();
        public void CheckInitialMapAnnouncements(float time)
        {
            if (time >= 90.0f * 1000)
            {
                NotifyMapAnnouncement(EventID.OnNexusCrystalStart, 0);
                AllAnnouncementsAnnounced = true;

            }
            if (time >= 80.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage2))
            {
                // The Battle Has Beguns!
                NotifyMapAnnouncement(EventID.OnStartGameMessage2, _map.Id);
                AnnouncedEvents.Add(EventID.OnStartGameMessage2);
            }
            else if (time >= 50.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage1))
            {
                // The battle will begin in 30 seconds!
                NotifyMapAnnouncement(EventID.OnStartGameMessage1, _map.Id);
                AnnouncedEvents.Add(EventID.OnStartGameMessage1);
            }
            else if (time >= 30.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage3))
            {
                // Welcome to the Crystal Scar!
                NotifyMapAnnouncement(EventID.OnStartGameMessage3, _map.Id);
                AnnouncedEvents.Add(EventID.OnStartGameMessage3);
            }
        }

        List<string> AnimationsNotified = new List<string>();
        bool NotifiedAllInitialAnimations = false;
        public void InitialBaseAnimations(float gameTime)
        {
            if (gameTime >= 87.0f * 1000 && !AnimationsNotified.Contains("Raised_Idle"))
            {
                foreach (var stair in TeamStairs.Values)
                {
                    NotifyPropAnimation(stair, "Raised_Idle", (AnimationFlags)1, 4.25f, false);
                }
                NotifiedAllInitialAnimations = true;
            }
            else if (gameTime >= 81.0f * 1000 && !AnimationsNotified.Contains("Raise"))
            {
                foreach (var stair in TeamStairs.Values)
                {
                    NotifyPropAnimation(stair, "Raise", (AnimationFlags)2, 6.7f, false);
                }
                AnimationsNotified.Add("Raise");
            }
            else if (gameTime >= 80.0f * 1000 && !AnimationsNotified.Contains("Particles4"))
            {
                //Blue Team Lasers
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_l_1_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_r_1_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_r_1_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_l_1_aim", teamOnly: TeamId.TEAM_PURPLE);

                //Purple Team Lasers
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_l_1_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_r_1_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_r_1_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_l_1_aim", teamOnly: TeamId.TEAM_BLUE);

                AnimationsNotified.Add("Particles4");
            }
            else if (gameTime >= 61.0f * 1000 && !AnimationsNotified.Contains("Close4"))
            {
                foreach (var stair in TeamStairs.Values)
                {
                    NotifyPropAnimation(stair, "Close4", (AnimationFlags)2, 20.0f, false);
                }
                AnimationsNotified.Add("Close4");
            }
            else if (gameTime >= 59.6f * 1000 && !AnimationsNotified.Contains("Particles3"))
            {
                //Blue Team Lasers
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_l_2_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_r_2_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_l_2_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_r_2_aim", teamOnly: TeamId.TEAM_PURPLE);

                //Purple Team Lasers
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_l_2_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_r_2_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_l_2_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_r_2_aim", teamOnly: TeamId.TEAM_BLUE);

                AnimationsNotified.Add("Particles3");
            }
            else if (gameTime >= 41.0f * 1000 && !AnimationsNotified.Contains("Close3"))
            {
                foreach (var stair in TeamStairs.Values)
                {
                    NotifyPropAnimation(stair, "Close3", (AnimationFlags)2, 20.0f, false);
                }
                AnimationsNotified.Add("Close3");
            }
            else if (gameTime >= 40.0f * 1000 && !AnimationsNotified.Contains("Particles2"))
            {
                //Blue Team Lasers
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_BLUE], 25000, 1, "Crystal_l_3_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_BLUE], 25000, 1, "Crystal_r_3_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_BLUE], 25000, 1, "Crystal_l_3_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_BLUE], 25000, 1, "Crystal_r_3_aim", teamOnly: TeamId.TEAM_PURPLE);

                //Purple Team Lasers
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_l_3_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_r_3_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_l_3_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_r_3_aim", teamOnly: TeamId.TEAM_BLUE);

                AnimationsNotified.Add("Particles2");
            }
            else if (gameTime >= 21.0f * 1000 && !AnimationsNotified.Contains("Close2"))
            {
                foreach (var stair in TeamStairs.Values)
                {
                    NotifyPropAnimation(stair, "Close2", (AnimationFlags)2, 20.0f, false);
                }
                AnimationsNotified.Add("Close2");
            }
            //The timing feels off, but from what i've seen from old footage, it looks like it is just like that
            else if (gameTime >= 16.0f * 1000 && !AnimationsNotified.Contains("Particles1"))
            {
                //Blue Team Lasers
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_l_4_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_r_4_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_l_4_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], TeamStairs[TeamId.TEAM_BLUE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_BLUE], 25000.0f, 1, "Crystal_r_4_aim", teamOnly: TeamId.TEAM_PURPLE);

                //Purple Team Lasers
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_l_4_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_green", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_r_4_aim", teamOnly: TeamId.TEAM_PURPLE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_l_4_aim", teamOnly: TeamId.TEAM_BLUE);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], TeamStairs[TeamId.TEAM_PURPLE], "odin_crystal_beam_red", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 1, "chaos_Crystal_r_4_aim", teamOnly: TeamId.TEAM_BLUE);

                //Center Crystal Particles
                AddParticleTarget(Nexus[TeamId.TEAM_BLUE], Nexus[TeamId.TEAM_BLUE], "Odin_crystal_beam_hit_blue", Nexus[TeamId.TEAM_BLUE], 25000.0f, 2);
                AddParticleTarget(Nexus[TeamId.TEAM_PURPLE], Nexus[TeamId.TEAM_PURPLE], "Odin_crystal_beam_hit_purple", Nexus[TeamId.TEAM_PURPLE], 25000.0f, 2);

                AnimationsNotified.Add("Particles1");
            }
            else if (gameTime >= 1000.0f && !AnimationsNotified.Contains("Close1"))
            {
                foreach (var stair in TeamStairs.Values)
                {
                    NotifyPropAnimation(stair, "Close1", (AnimationFlags)2, 17.5f, false);
                }
                AnimationsNotified.Add("Close1");
            }
        }
    }
}