using System;
using System.Collections.Generic;
using System.Numerics;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using GameServerCore.Maps;
using GameServerCore.Scripting.CSharp;
using LeagueSandbox.GameServer.API;
using LeagueSandbox.GameServer.Content;
using LeagueSandbox.GameServer.GameObjects.Stats;
using LeagueSandbox.GameServer.Scripting.CSharp;
using static GameServerLib.API.APIMapFunctionManager;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace MapScripts.Map12
{
    public class ARAM : IMapScript
    {
        public IMapScriptMetadata MapScriptMetadata { get; set; } = new MapScriptMetadata
        {
            StartingGold = 1375.0f,
            GoldPerSecond = 5.0f,
            RecallSpellItemId = 2007,
            EnableFountainHealing = false,
            EnableBuildingProtection = true,
            InitialLevel = 3
        };

        private IMapScriptHandler _map;
        public virtual IGlobalData GlobalData { get; set; } = new GlobalData();
        public bool HasFirstBloodHappened { get; set; } = false;
        public long NextSpawnTime { get; set; } = 60 * 1000;
        public string LaneMinionAI { get; set; } = "LaneMinionAI";
        public string LaneTurretAI { get; set; } = "TurretAI";
        public Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> PlayerSpawnPoints { get; }

        //Tower type enumeration might vary slightly from map to map, so we set that up here
        public TurretType GetTurretType(int trueIndex, LaneID lane, TeamId teamId)
        {
            TurretType returnType = TurretType.FOUNTAIN_TURRET;

            switch (trueIndex)
            {
                case 3:
                case 4:
                case 9:
                case 10:
                    returnType = TurretType.NEXUS_TURRET;
                    break;
                case 2:
                case 7:
                    returnType = TurretType.INHIBITOR_TURRET;
                    break;
                case 1:
                case 8:
                    returnType = TurretType.INNER_TURRET;
                    break;
            }

            return returnType;
        }

        //Nexus models
        public Dictionary<TeamId, string> NexusModels { get; set; } = new Dictionary<TeamId, string>
        {
            {TeamId.TEAM_BLUE, "ARAMOrderNexus" },
            {TeamId.TEAM_PURPLE, "ARAMChaosNexus" }
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
                {TurretType.FOUNTAIN_TURRET, "HA_AP_OrderShrineTurret" },
                {TurretType.NEXUS_TURRET, "HA_AP_OrderTurret3" },
                {TurretType.INHIBITOR_TURRET, "HA_AP_OrderTurret2" },
                {TurretType.INNER_TURRET, "HA_AP_OrderTurret" },
            } },
            {TeamId.TEAM_PURPLE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "HA_AP_ChaosTurretShrine" },
                {TurretType.NEXUS_TURRET, "HA_AP_ChaosTurret3" },
                {TurretType.INHIBITOR_TURRET, "HA_AP_ChaosTurret2" },
                {TurretType.INNER_TURRET, "HA_AP_ChaosTurret" },
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

        public Dictionary<int, ILevelProp> Poros = new Dictionary<int, ILevelProp>();
        public Dictionary<int, ILevelProp> LongChains = new Dictionary<int, ILevelProp>();
        public Dictionary<int, ILevelProp> Chains = new Dictionary<int, ILevelProp>();
        public void Init(IMapScriptHandler map)
        {
            _map = map;

            MapScriptMetadata.MinionSpawnEnabled = IsMinionSpawnEnabled();

            foreach (var key in map.SpawnBarracks.Keys)
            {
                if (!key.Contains("____P"))
                {
                    map.SpawnBarracks.Remove(key);
                }
            }

            AddSurrender(1200000.0f, 300000.0f, 30.0f);

            //Due to riot's questionable map-naming scheme some towers are missplaced into other lanes during outomated setup, so we have to manually fix them.
            ChangeTowerOnMapList("Turret_T2_L_04_A", TeamId.TEAM_PURPLE, LaneID.TOP, LaneID.MIDDLE);
            ChangeTowerOnMapList("Turret_T2_L_01_A", TeamId.TEAM_PURPLE, LaneID.TOP, LaneID.MIDDLE);
            ChangeTowerOnMapList("Turret_T2_L_02_A", TeamId.TEAM_PURPLE, LaneID.TOP, LaneID.MIDDLE);
            ChangeTowerOnMapList("Turret_T2_L_03_A", TeamId.TEAM_PURPLE, LaneID.TOP, LaneID.MIDDLE);

            CreateLevelProps.CreateProps(this);
        }

        IStatsModifier TurretStatsModifier = new StatsModifier();
        Dictionary<TeamId, List<IChampion>> Players = new Dictionary<TeamId, List<IChampion>>();
        public void OnMatchStart()
        {
            foreach (var nexus in _map.NexusList)
            {
                ApiEventManager.OnDeath.AddListener(this, nexus, OnNexusDeath, true);
            }

            foreach (var champion in GetAllPlayers())
            {
                AddBuff("HowlingAbyssAura", 25000, 1, null, champion, null);
            }

            Players.Add(TeamId.TEAM_BLUE, GetAllPlayersFromTeam(TeamId.TEAM_BLUE));
            Players.Add(TeamId.TEAM_PURPLE, GetAllPlayersFromTeam(TeamId.TEAM_PURPLE));

            IStatsModifier TurretHealthModifier = new StatsModifier();
            foreach (var team in _map.TurretList.Keys)
            {
                TeamId enemyTeam = TeamId.TEAM_BLUE;

                if (team == TeamId.TEAM_BLUE)
                {
                    enemyTeam = TeamId.TEAM_PURPLE;
                }

                TurretHealthModifier.HealthPoints.BaseBonus = 250.0f * Players[enemyTeam].Count;

                foreach (var lane in _map.TurretList[team].Keys)
                {
                    foreach (var turret in _map.TurretList[team][lane])
                    {
                        if (turret.Type == TurretType.FOUNTAIN_TURRET)
                        {
                            continue;
                        }

                        turret.AddStatModifier(TurretHealthModifier);
                        turret.Stats.CurrentHealth += turret.Stats.HealthPoints.Total;
                    }
                }
            }

            TurretStatsModifier.Armor.FlatBonus = 1;
            TurretStatsModifier.MagicResist.FlatBonus = 1;
            TurretStatsModifier.AttackDamage.FlatBonus = 6;

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

            NeutralMinionSpawn.InitializeJungle();
        }

        //This function gets executed every server tick
        public void Update(float diff)
        {
            NeutralMinionSpawn.OnUpdate(diff);

            var gameTime = GameTime();
            if (!AllAnnouncementsAnnounced)
            {
                CheckMapInitialAnnouncements(gameTime);
            }

            if (gameTime >= timeCheck && timesApplied < 8)
            {
                UpdateTowerStats();
            }
        }

        float timeCheck = 0.0f * 1000;
        byte timesApplied = 0;
        public void UpdateTowerStats()
        {
            foreach (var team in _map.TurretList.Keys)
            {
                foreach (var lane in _map.TurretList[team].Keys)
                {
                    foreach (var turret in _map.TurretList[team][lane])
                    {
                        if (turret.Type == TurretType.FOUNTAIN_TURRET)
                        {
                            continue;
                        }

                        turret.AddStatModifier(TurretStatsModifier);
                    }
                }
            }

            timesApplied++;
            timeCheck += 60.0f * 1000;
        }

        public void OnNexusDeath(IDeathData deathaData)
        {
            var nexus = deathaData.Unit;
            EndGame(nexus.Team, new Vector3(nexus.Position.X, nexus.GetHeight(), nexus.Position.Y), deathData: deathaData);
        }

        public void SpawnAllCamps()
        {
            NeutralMinionSpawn.ForceCampSpawn();
        }

        bool AllAnnouncementsAnnounced = false;
        List<EventID> AnnouncedEvents = new List<EventID>();
        public void CheckMapInitialAnnouncements(float time)
        {
            if (time >= 60.0f * 1000)
            {
                // Minions have spawned
                NotifyMapAnnouncement(EventID.OnMinionsSpawn, 0);
                NotifyMapAnnouncement(EventID.OnNexusCrystalStart, 0);
                AllAnnouncementsAnnounced = true;
            }
            else if (time >= 30.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage1))
            {
                // Welcome to the Howling Abyss
                NotifyMapAnnouncement(EventID.OnStartGameMessage1, _map.Id);
                AnnouncedEvents.Add(EventID.OnStartGameMessage1);
            }
        }
    }
}
