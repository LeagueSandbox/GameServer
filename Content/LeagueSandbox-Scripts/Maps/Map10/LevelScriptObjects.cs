﻿using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using System.Collections.Generic;
using GameServerCore.Domain;
using System.Numerics;
using System.Linq;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace MapScripts.Map10
{
    public static class LevelScriptObjects
    {
        private static Dictionary<GameObjectTypes, List<IMapObject>> _mapObjects;

        public static Dictionary<TeamId, IFountain> FountainList = new Dictionary<TeamId, IFountain>();
        public static Dictionary<TeamId, Dictionary<LaneID, IMapObject>> SpawnBarracks = new Dictionary<TeamId, Dictionary<LaneID, IMapObject>>();
        public static Dictionary<LaneID, List<Vector2>> MinionPaths = new Dictionary<LaneID, List<Vector2>> { { LaneID.TOP, new List<Vector2>() }, { LaneID.BOTTOM, new List<Vector2>() } };
        static List<INexus> NexusList = new List<INexus>();
        static string LaneTurretAI = "TurretAI";

        static Dictionary<TeamId, Dictionary<LaneID, List<ILaneTurret>>> TurretList = new Dictionary<TeamId, Dictionary<LaneID, List<ILaneTurret>>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<LaneID, List<ILaneTurret>>{
                { LaneID.NONE, new List<ILaneTurret>()},
                { LaneID.TOP, new List<ILaneTurret>()},
                { LaneID.MIDDLE, new List<ILaneTurret>()},
                { LaneID.BOTTOM, new List<ILaneTurret>()}}
            },
            {TeamId.TEAM_PURPLE, new Dictionary<LaneID, List<ILaneTurret>>{
                { LaneID.NONE, new List<ILaneTurret>()},
                { LaneID.TOP, new List<ILaneTurret>()},
                { LaneID.MIDDLE, new List<ILaneTurret>()},
                { LaneID.BOTTOM, new List<ILaneTurret>()}}
            }
        };

        public static Dictionary<TeamId, Dictionary<LaneID, IInhibitor>> InhibitorList = new Dictionary<TeamId, Dictionary<LaneID, IInhibitor>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<LaneID, IInhibitor>{
                { LaneID.TOP, null },
                { LaneID.BOTTOM, null }}
            },
            {TeamId.TEAM_PURPLE, new Dictionary<LaneID, IInhibitor>{
                { LaneID.TOP, null },
                { LaneID.BOTTOM, null }}
            }
        };

        //Nexus models
        static Dictionary<TeamId, string> NexusModels { get; set; } = new Dictionary<TeamId, string>
        {
            {TeamId.TEAM_BLUE, "OrderNexus" },
            {TeamId.TEAM_PURPLE, "ChaosNexus" }
        };

        //Inhib models
        static Dictionary<TeamId, string> InhibitorModels { get; set; } = new Dictionary<TeamId, string>
        {
            {TeamId.TEAM_BLUE, "TT_OrderInhibitor" },
            {TeamId.TEAM_PURPLE, "TT_ChaosInhibitor" }
        };

        //Tower Models
        static Dictionary<TeamId, Dictionary<TurretType, string>> TowerModels { get; set; } = new Dictionary<TeamId, Dictionary<TurretType, string>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "TT_OrderTurret4" },
                {TurretType.NEXUS_TURRET, "TT_OrderTurret3" },
                {TurretType.INHIBITOR_TURRET, "TT_OrderTurret1" },
                {TurretType.INNER_TURRET, "TT_OrderTurret2" }
            } },
            {TeamId.TEAM_PURPLE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "TT_ChaosTurret4" },
                {TurretType.NEXUS_TURRET, "TT_ChaosTurret3" },
                {TurretType.INHIBITOR_TURRET, "TT_ChaosTurret1" },
                {TurretType.INNER_TURRET, "TT_ChaosTurret2" }
            } }
        };

        //Turret Items
        public static Dictionary<TurretType, int[]> TurretItems { get; set; } = new Dictionary<TurretType, int[]>
        {
            { TurretType.OUTER_TURRET, new[] { 1500, 1501, 1502, 1503 } },
            { TurretType.INNER_TURRET, new[] { 1500, 1501, 1502, 1503, 1504 } },
            { TurretType.INHIBITOR_TURRET, new[] { 1501, 1502, 1503, 1505 } },
            { TurretType.NEXUS_TURRET, new[] { 1501, 1502, 1503, 1505 } }
        };

        static IStatsModifier TurretStatsModifier = new StatsModifier();
        public static void LoadObjects(Dictionary<GameObjectTypes, List<IMapObject>> mapObjects)
        {
            _mapObjects = mapObjects;

            CreateBuildings();
            ChangeTowerOnMapList(TurretList, "Turret_T1_C_07_A", TeamId.TEAM_BLUE, LaneID.MIDDLE, LaneID.BOTTOM);
            ChangeTowerOnMapList(TurretList, "Turret_T1_C_06_A", TeamId.TEAM_BLUE, LaneID.MIDDLE, LaneID.TOP);
            LoadProtection();

            LoadSpawnBarracks();
            LoadMinionPathing();
            LoadFountains();
        }

        static void LoadMinionPathing()
        {
            foreach (var minionPath in _mapObjects[GameObjectTypes.ObjBuilding_NavPoint])
            {
                var lane = minionPath.GetLaneID();

                MinionPaths[lane].Add(new Vector2(minionPath.CentralPoint.X, minionPath.CentralPoint.Z));
            }
        }

        static Dictionary<TeamId, List<IChampion>> Players = new Dictionary<TeamId, List<IChampion>>();
        public static void OnMatchStart()
        {
            LoadShops();

            foreach (var nexus in NexusList)
            {
                ApiEventManager.OnDeath.AddListener(nexus, nexus, OnNexusDeath, true);
            }

            Players.Add(TeamId.TEAM_BLUE, ApiFunctionManager.GetAllPlayersFromTeam(TeamId.TEAM_BLUE));
            Players.Add(TeamId.TEAM_PURPLE, ApiFunctionManager.GetAllPlayersFromTeam(TeamId.TEAM_PURPLE));

            IStatsModifier TurretHealthModifier = new StatsModifier();
            foreach (var team in TurretList.Keys)
            {
                TeamId enemyTeam = TeamId.TEAM_BLUE;

                if (team == TeamId.TEAM_BLUE)
                {
                    enemyTeam = TeamId.TEAM_PURPLE;
                }

                TurretHealthModifier.HealthPoints.BaseBonus = 250.0f * Players[enemyTeam].Count;

                foreach (var lane in TurretList[team].Keys)
                {
                    foreach (var turret in TurretList[team][lane])
                    {
                        if (turret.Type == TurretType.FOUNTAIN_TURRET)
                        {
                            continue;
                        }

                        turret.AddStatModifier(TurretHealthModifier);
                        turret.Stats.CurrentHealth += turret.Stats.HealthPoints.Total;
                        AddTurretItems(turret, GetTurretItems(TurretItems, turret.Type));
                    }
                }
            }

            TurretStatsModifier.Armor.FlatBonus = 1;
            TurretStatsModifier.MagicResist.FlatBonus = 1;
            TurretStatsModifier.AttackDamage.FlatBonus = 4;

        }

        public static void OnUpdate(float diff)
        {
            var gameTime = GameTime();

            if (gameTime >= timeCheck && timesApplied < 30)
            {
                UpdateTowerStats();
            }

            foreach (var fountain in FountainList.Values)
            {
                fountain.Update(diff);
            }
        }

        static void OnNexusDeath(IDeathData deathaData)
        {
            var nexus = deathaData.Unit;
            EndGame(nexus.Team, new Vector3(nexus.Position.X, nexus.GetHeight(), nexus.Position.Y), deathData: deathaData);
        }

        static float timeCheck = 480.0f * 1000;
        static byte timesApplied = 0;
        static void UpdateTowerStats()
        {
            foreach (var team in TurretList.Keys)
            {
                foreach (var lane in TurretList[team].Keys)
                {
                    foreach (var turret in TurretList[team][lane])
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

        static void LoadFountains()
        {
            foreach (var fountain in _mapObjects[GameObjectTypes.ObjBuilding_SpawnPoint])
            {
                var team = fountain.GetTeamID();
                FountainList.Add(team, CreateFountain(team, new Vector2(fountain.CentralPoint.X, fountain.CentralPoint.Z)));
            }
        }

        static void LoadShops()
        {
            foreach (var shop in _mapObjects[GameObjectTypes.ObjBuilding_Shop])
            {
                NotifySpawn(CreateShop(shop.Name, new Vector2(shop.CentralPoint.X, shop.CentralPoint.Z), shop.GetTeamID()));
            }
        }

        static void LoadSpawnBarracks()
        {
            foreach (var spawnBarrack in _mapObjects[GameObjectTypes.ObjBuildingBarracks])
            {
                var team = spawnBarrack.GetTeamID();
                if (!SpawnBarracks.ContainsKey(team))
                {
                    SpawnBarracks.Add(team, new Dictionary<LaneID, IMapObject>());
                }
                SpawnBarracks[team].Add(spawnBarrack.GetLaneID(), spawnBarrack);
            }
        }

        static void CreateBuildings()
        {
            foreach (var nexusObj in _mapObjects[GameObjectTypes.ObjAnimated_HQ])
            {
                var teamId = nexusObj.GetTeamID();
                var position = new Vector2(nexusObj.CentralPoint.X, nexusObj.CentralPoint.Z);

                var nexus = CreateNexus(nexusObj.Name, NexusModels[teamId], position, teamId, 353, 1700);
                NexusList.Add(nexus);
                AddObject(nexus);
            }

            foreach (var inhibitorObj in _mapObjects[GameObjectTypes.ObjAnimated_BarracksDampener])
            {
                var teamId = inhibitorObj.GetTeamID();
                var lane = inhibitorObj.GetLaneID();
                var position = new Vector2(inhibitorObj.CentralPoint.X, inhibitorObj.CentralPoint.Z);

                var inhibitor = CreateInhibitor(inhibitorObj.Name, InhibitorModels[teamId], position, teamId, lane, 214, 0);
                InhibitorList[teamId][lane] = inhibitor;
                AddObject(inhibitor);
            }

            foreach (var turretObj in _mapObjects[GameObjectTypes.ObjAIBase_Turret])
            {
                var teamId = turretObj.GetTeamID();
                var lane = turretObj.GetLaneID();
                var position = new Vector2(turretObj.CentralPoint.X, turretObj.CentralPoint.Z);

                if (turretObj.Name.Contains("Shrine"))
                {
                    var fountainTurret = CreateLaneTurret(turretObj.Name + "_A", TowerModels[teamId][TurretType.FOUNTAIN_TURRET], position, teamId, TurretType.FOUNTAIN_TURRET, LaneID.NONE, LaneTurretAI, turretObj);
                    TurretList[teamId][lane].Add(fountainTurret);
                    AddObject(fountainTurret);
                    continue;
                }

                var turretType = GetTurretType(turretObj.ParseIndex(), lane, teamId);
                LogInfo(turretObj.Name + ": " + turretType.ToString());

                var turret = CreateLaneTurret(turretObj.Name + "_A", TowerModels[teamId][turretType], position, teamId, turretType, lane, LaneTurretAI, turretObj);
                TurretList[teamId][lane].Add(turret);
                AddObject(turret);
            }
        }

        static TurretType GetTurretType(int trueIndex, LaneID lane, TeamId teamId)
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

        static void LoadProtection()
        {
            //I can't help but feel there's a better way to do this
            Dictionary<TeamId, List<IInhibitor>> TeamInhibitors = new Dictionary<TeamId, List<IInhibitor>> { { TeamId.TEAM_BLUE, new List<IInhibitor>() }, { TeamId.TEAM_PURPLE, new List<IInhibitor>() } };
            foreach (var teams in InhibitorList.Keys)
            {
                foreach (var lane in InhibitorList[teams].Keys)
                {
                    TeamInhibitors[teams].Add(InhibitorList[teams][lane]);
                }
            }

            foreach (var nexus in NexusList)
            {
                // Adds Protection to Nexus
                AddProtection(nexus, TurretList[nexus.Team][LaneID.MIDDLE].FindAll(turret => turret.Type == TurretType.NEXUS_TURRET).ToArray(), TeamInhibitors[nexus.Team].ToArray());
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
                        AddProtection(inhibitor, false, inhibitorTurret);
                    }

                    // Adds Protection to Turrets
                    foreach (var turret in TurretList[inhibitor.Team][inhibitor.Lane])
                    {
                        AddProtection(turret, false, TurretList[inhibitor.Team][inhibitor.Lane].First(dependTurret => dependTurret.Type == TurretType.INNER_TURRET));
                    }
                }
                foreach (var turret in TurretList[InhibTeam][LaneID.MIDDLE])
                {
                    AddProtection(turret, false, TeamInhibitors[InhibTeam].ToArray());
                }
            }
        }
    }
}
