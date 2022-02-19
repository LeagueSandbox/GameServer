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

namespace MapScripts.Map10
{
    public class CLASSIC : IMapScript
    {
        public IMapScriptMetadata MapScriptMetadata { get; set; } = new MapScriptMetadata
        {
            EnableBuildingProtection = true,
            StartingGold = 825.0f
        };

        private IMapScriptHandler _map;
        public virtual IGlobalData GlobalData { get; set; } = new GlobalData();
        public bool HasFirstBloodHappened { get; set; } = false;
        public long NextSpawnTime { get; set; } = 45 * 1000;
        public string LaneMinionAI { get; set; } = "LaneMinionAI";
        public string LaneTurretAI { get; set; } = "TurretAI";

        public Dictionary<TeamId, Dictionary<int, Dictionary<int, Vector2>>> PlayerSpawnPoints { get; }

        //Tower type enumeration might vary slightly from map to map, so we set that up here
        public TurretType GetTurretType(int trueIndex, LaneID lane, TeamId teamId)
        {
            TurretType returnType = TurretType.NEXUS_TURRET;
            switch (trueIndex)
            {
                case 1:
                case 6:
                case 7:
                    returnType = TurretType.INHIBITOR_TURRET;
                    break;
                case 2:
                    returnType = TurretType.INNER_TURRET;
                    break;
            }

            if (trueIndex == 1 && lane == LaneID.MIDDLE)
            {
                returnType = TurretType.NEXUS_TURRET;
            }

            return returnType;
        }

        //Nexus models
        public Dictionary<TeamId, string> NexusModels { get; set; } = new Dictionary<TeamId, string>
        {
            {TeamId.TEAM_BLUE, "OrderNexus" },
            {TeamId.TEAM_PURPLE, "ChaosNexus" }
        };
        //Inhib models
        public Dictionary<TeamId, string> InhibitorModels { get; set; } = new Dictionary<TeamId, string>
        {
            {TeamId.TEAM_BLUE, "TT_OrderInhibitor" },
            {TeamId.TEAM_PURPLE, "TT_ChaosInhibitor" }
        };
        //Tower Models
        public Dictionary<TeamId, Dictionary<TurretType, string>> TowerModels { get; set; } = new Dictionary<TeamId, Dictionary<TurretType, string>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "TT_OrderTurret4" },
                {TurretType.NEXUS_TURRET, "TT_OrderTurret3" },
                {TurretType.INHIBITOR_TURRET, "TT_OrderTurret1" },
                {TurretType.INNER_TURRET, "TT_OrderTurret2" },
            } },
            {TeamId.TEAM_PURPLE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "TT_ChaosTurret4" },
                {TurretType.NEXUS_TURRET, "TT_ChaosTurret3" },
                {TurretType.INHIBITOR_TURRET, "TT_ChaosTurret1" },
                {TurretType.INNER_TURRET, "TT_ChaosTurret2" },
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

        //This function is executed in-between Loading the map structures and applying the structure protections. Is the first thing on this script to be executed
        public void Init(IMapScriptHandler map)
        {
            _map = map;

            MapScriptMetadata.MinionSpawnEnabled = IsMinionSpawnEnabled();
            AddSurrender(1200000.0f, 300000.0f, 30.0f);

            //Due to riot's questionable map-naming scheme some towers are missplaced into other lanes during outomated setup, so we have to manually fix them.
            ChangeTowerOnMapList("Turret_T1_C_07_A", TeamId.TEAM_BLUE, LaneID.MIDDLE, LaneID.BOTTOM);
            ChangeTowerOnMapList("Turret_T1_C_06_A", TeamId.TEAM_BLUE, LaneID.MIDDLE, LaneID.TOP);

            CreateLevelProps.CreateProps();
        }

        IStatsModifier TurretStatsModifier = new StatsModifier();
        Dictionary<TeamId, List<IChampion>> Players = new Dictionary<TeamId, List<IChampion>>();
        public void OnMatchStart()
        {
            foreach (var nexus in _map.NexusList)
            {
                ApiEventManager.OnDeath.AddListener(this, nexus, OnNexusDeath, true);
            }

            Players.Add(TeamId.TEAM_BLUE, ApiFunctionManager.GetAllPlayersFromTeam(TeamId.TEAM_BLUE));
            Players.Add(TeamId.TEAM_PURPLE, ApiFunctionManager.GetAllPlayersFromTeam(TeamId.TEAM_PURPLE));

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
            TurretStatsModifier.AttackDamage.FlatBonus = 4;

            NeutralMinionSpawn.InitializeCamps();
        }

        //This function gets executed every server tick
        public void Update(float diff)
        {
            NeutralMinionSpawn.OnUpdate(diff);

            float gameTime = GameTime();
            if (!AllAnnouncementsAnnounced)
            {
                CheckInitialMapAnnouncements(gameTime);
            }

            if (gameTime >= timeCheck && timesApplied < 30)
            {
                UpdateTowerStats();
            }

        }

        float timeCheck = 480.0f * 1000;
        byte timesApplied = 0;
        public void UpdateTowerStats()
        {
            foreach (var team in _map.TurretList.Keys)
            {
                foreach (var lane in _map.TurretList[team].Keys)
                {
                    foreach (var turret in _map.TurretList[team][lane])
                    {
                        if (turret.Type == TurretType.FOUNTAIN_TURRET || ((turret.Type != TurretType.NEXUS_TURRET) && timesApplied >= 20))
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
        public void CheckInitialMapAnnouncements(float time)
        {
            if (time >= 180.0f * 1000)
            {
                //The Altars have unlocked!
                NotifyMapAnnouncement(EventID.OnStartGameMessage4, _map.Id);
                AllAnnouncementsAnnounced = true;
            }
            else if (time >= 150.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage2))
            {
                // The Altars will unlock in 30 seconds
                NotifyMapAnnouncement(EventID.OnStartGameMessage2, _map.Id);
                AnnouncedEvents.Add(EventID.OnStartGameMessage2);

            }
            else if (time >= 75.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage3))
            {
                // Minions have Spawned
                NotifyMapAnnouncement(EventID.OnStartGameMessage3, _map.Id);
                NotifyMapAnnouncement(EventID.OnNexusCrystalStart, 0);
                AnnouncedEvents.Add(EventID.OnStartGameMessage3);
            }
            else if (time >= 30.0f * 1000 && !AnnouncedEvents.Contains(EventID.OnStartGameMessage1))
            {
                // Welcome to the Twisted Tree Line!
                NotifyMapAnnouncement(EventID.OnStartGameMessage1, _map.Id);
                AnnouncedEvents.Add(EventID.OnStartGameMessage1);
            }
        }
    }
}
