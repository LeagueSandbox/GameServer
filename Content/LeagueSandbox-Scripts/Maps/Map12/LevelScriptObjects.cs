using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using System.Collections.Generic;
using GameServerCore.Domain;
using System.Numerics;
using System.Linq;

namespace MapScripts.Map12
{
    public static class LevelScriptObjects
    {
        private static Dictionary<GameObjectTypes, List<MapObject>> _mapObjects;

        public static Dictionary<TeamId, IFountain> FountainList = new Dictionary<TeamId, IFountain>();
        public static Dictionary<TeamId, Dictionary<LaneID, MapObject>> SpawnBarracks = new Dictionary<TeamId, Dictionary<LaneID, MapObject>> { { TeamId.TEAM_BLUE, new Dictionary<LaneID, MapObject>() }, { TeamId.TEAM_PURPLE, new Dictionary<LaneID, MapObject>() } };
        public static Dictionary<LaneID, List<Vector2>> MinionPaths = new Dictionary<LaneID, List<Vector2>> { { LaneID.MIDDLE, new List<Vector2>() } };
        static Dictionary<TeamId, Dictionary<IInhibitor, float>> DeadInhibitors = new Dictionary<TeamId, Dictionary<IInhibitor, float>> { { TeamId.TEAM_BLUE, new Dictionary<IInhibitor, float>() }, { TeamId.TEAM_PURPLE, new Dictionary<IInhibitor, float>() } };
        static List<INexus> NexusList = new List<INexus>();
        static string LaneTurretAI = "TurretAI";

        static Dictionary<TeamId, Dictionary<LaneID, List<ILaneTurret>>> TurretList = new Dictionary<TeamId, Dictionary<LaneID, List<ILaneTurret>>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<LaneID, List<ILaneTurret>>{
                { LaneID.NONE, new List<ILaneTurret>()},
                { LaneID.MIDDLE, new List<ILaneTurret>()} }
            },
            {TeamId.TEAM_PURPLE, new Dictionary<LaneID, List<ILaneTurret>>{
                { LaneID.NONE, new List<ILaneTurret>()},
                { LaneID.MIDDLE, new List<ILaneTurret>()},
            } }
        };

        public static Dictionary<TeamId, IInhibitor> InhibitorList = new Dictionary<TeamId, IInhibitor>();

        //Nexus models
        public static Dictionary<TeamId, string> NexusModels { get; set; } = new Dictionary<TeamId, string>
        {
            {TeamId.TEAM_BLUE, "ARAMOrderNexus" },
            {TeamId.TEAM_PURPLE, "ARAMChaosNexus" }
        };

        //Inhib models
        public static Dictionary<TeamId, string> InhibitorModels { get; set; } = new Dictionary<TeamId, string>
        {
            {TeamId.TEAM_BLUE, "OrderInhibitor" },
            {TeamId.TEAM_PURPLE, "ChaosInhibitor" }
        };

        //Tower Models
        public static Dictionary<TeamId, Dictionary<TurretType, string>> TowerModels { get; set; } = new Dictionary<TeamId, Dictionary<TurretType, string>>
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
        public static Dictionary<TurretType, int[]> TurretItems { get; set; } = new Dictionary<TurretType, int[]>
        {
            { TurretType.OUTER_TURRET, new[] { 1500, 1501, 1502, 1503 } },
            { TurretType.INNER_TURRET, new[] { 1500, 1501, 1502, 1503, 1504 } },
            { TurretType.INHIBITOR_TURRET, new[] { 1501, 1502, 1503, 1505 } },
            { TurretType.NEXUS_TURRET, new[] { 1501, 1502, 1503, 1505 } }
        };

        static IStatsModifier TurretStatsModifier = new StatsModifier();
        public static void LoadObjects(Dictionary<GameObjectTypes, List<MapObject>> mapObjects)
        {
            _mapObjects = mapObjects;

            CreateBuildings();
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

        public static void OnMatchStart()
        {
            LoadShops();

            Dictionary<TeamId, List<IChampion>> Players = new Dictionary<TeamId, List<IChampion>>
            {
                {TeamId.TEAM_BLUE, ApiFunctionManager.GetAllPlayersFromTeam(TeamId.TEAM_BLUE) },
                {TeamId.TEAM_PURPLE, ApiFunctionManager.GetAllPlayersFromTeam(TeamId.TEAM_PURPLE) }
            };

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
            TurretStatsModifier.AttackDamage.FlatBonus = 6;
        }

        public static void OnUpdate(float diff)
        {
            var gameTime = GameTime();

            if (gameTime >= timeCheck && timesApplied < 8)
            {
                UpdateTowerStats();
            }

            foreach (var team in DeadInhibitors.Keys)
            {
                foreach (var inhibitor in DeadInhibitors[team].Keys.ToList())
                {
                    DeadInhibitors[team][inhibitor] -= diff;
                    if (DeadInhibitors[team][inhibitor] <= 0)
                    {
                        inhibitor.Stats.CurrentHealth = inhibitor.Stats.HealthPoints.Total;
                        inhibitor.NotifyState();
                        DeadInhibitors[inhibitor.Team].Remove(inhibitor);
                    }
                    else if (DeadInhibitors[team][inhibitor] <= 15.0f * 1000)
                    {
                        inhibitor.SetState(InhibitorState.ALIVE);
                    }
                }
            }
        }

        static void OnNexusDeath(IDeathData deathaData)
        {
            var nexus = deathaData.Unit;
            EndGame(nexus.Team, new Vector3(nexus.Position.X, nexus.GetHeight(), nexus.Position.Y), deathData: deathaData);
        }

        public static void OnInhibitorDeath(IDeathData deathData)
        {
            var inhibitor = deathData.Unit as IInhibitor;

            DeadInhibitors[inhibitor.Team].Add(inhibitor, inhibitor.RespawnTime * 1000);
        }

        static float timeCheck = 0.0f * 1000;
        static byte timesApplied = 0;
        static void UpdateTowerStats()
        {
            foreach (var team in TurretList.Keys)
            {
                foreach (var lane in TurretList[team].Keys)
                {
                    foreach (var turret in TurretList[team][lane])
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
                CreateShop(shop.Name, new Vector2(shop.CentralPoint.X, shop.CentralPoint.Z), shop.GetTeamID());
            }
        }

        static void LoadSpawnBarracks()
        {
            foreach (var spawnBarrack in _mapObjects[GameObjectTypes.ObjBuildingBarracks])
            {
                if (spawnBarrack.Name.Contains("____P"))
                {
                    SpawnBarracks[spawnBarrack.GetTeamID()].Add(spawnBarrack.GetLaneID(), spawnBarrack);
                }
            }
        }

        static void CreateBuildings()
        {
            foreach (var nexusObj in _mapObjects[GameObjectTypes.ObjAnimated_HQ])
            {
                var teamId = nexusObj.GetTeamID();
                var position = new Vector2(nexusObj.CentralPoint.X, nexusObj.CentralPoint.Z);

                var nexus = CreateNexus(nexusObj.Name, NexusModels[teamId], position, teamId, 353, 1700);
                ApiEventManager.OnDeath.AddListener(nexus, nexus, OnNexusDeath, true);
                NexusList.Add(nexus);
                AddObject(nexus);
            }

            foreach (var inhibitorObj in _mapObjects[GameObjectTypes.ObjAnimated_BarracksDampener])
            {
                var teamId = inhibitorObj.GetTeamID();
                var lane = inhibitorObj.GetLaneID();
                var position = new Vector2(inhibitorObj.CentralPoint.X, inhibitorObj.CentralPoint.Z);

                var inhibitor = CreateInhibitor(inhibitorObj.Name, InhibitorModels[teamId], position, teamId, lane, 214, 0);
                ApiEventManager.OnDeath.AddListener(inhibitor, inhibitor, OnInhibitorDeath, false);
                inhibitor.RespawnTime = 300.0f;
                inhibitor.Stats.CurrentHealth = 4000.0f;
                inhibitor.Stats.HealthPoints.BaseValue = 4000.0f;
                InhibitorList.Add(teamId, inhibitor);
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

                var turretType = GetTurretType(turretObj.ParseIndex());

                var turret = CreateLaneTurret(turretObj.Name + "_A", TowerModels[teamId][turretType], position, teamId, turretType, LaneID.MIDDLE, LaneTurretAI, turretObj);
                TurretList[teamId][LaneID.MIDDLE].Add(turret);
                AddObject(turret);
            }
        }

        static TurretType GetTurretType(int trueIndex)
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

        static void LoadProtection()
        {
            //I can't help but feel there's a better way to do this
            Dictionary<TeamId, List<IInhibitor>> TeamInhibitors = new Dictionary<TeamId, List<IInhibitor>> { { TeamId.TEAM_BLUE, new List<IInhibitor>() }, { TeamId.TEAM_PURPLE, new List<IInhibitor>() } };
            foreach (var teams in InhibitorList.Keys)
            {
                TeamInhibitors[teams].Add(InhibitorList[teams]);
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
                        if (turret.Type == TurretType.NEXUS_TURRET)
                        {
                            AddProtection(turret, false, TeamInhibitors[inhibitor.Team].ToArray());
                        }
                        else if (turret.Type == TurretType.INHIBITOR_TURRET)
                        {
                            AddProtection(turret, false, TurretList[inhibitor.Team][inhibitor.Lane].First(dependTurret => dependTurret.Type == TurretType.INNER_TURRET));
                        }
                    }
                }
            }
        }
    }
}
