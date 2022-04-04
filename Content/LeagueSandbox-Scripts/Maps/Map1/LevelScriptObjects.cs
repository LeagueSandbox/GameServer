using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using LeagueSandbox.GameServer.API;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;
using LeagueSandbox.GameServer.GameObjects.Stats;
using System.Collections.Generic;
using GameServerCore.Domain;
using System.Numerics;
using System.Linq;

namespace MapScripts.Map1
{
    public static class LevelScriptObjects
    {
        private static Dictionary<GameObjectTypes, List<MapObject>> _mapObjects;

        public static Dictionary<TeamId, IFountain> FountainList = new Dictionary<TeamId, IFountain>();
        public static Dictionary<string, MapObject> SpawnBarracks = new Dictionary<string, MapObject>();
        public static Dictionary<TeamId, bool> AllInhibitorsAreDead = new Dictionary<TeamId, bool> { { TeamId.TEAM_BLUE, false }, { TeamId.TEAM_PURPLE, false } };
        static Dictionary<TeamId, Dictionary<IInhibitor, float>> DeadInhibitors = new Dictionary<TeamId, Dictionary<IInhibitor, float>> { { TeamId.TEAM_BLUE, new Dictionary<IInhibitor, float>() }, { TeamId.TEAM_PURPLE, new Dictionary<IInhibitor, float>() } };
        static List<INexus> NexusList = new List<INexus>();
        public static string LaneTurretAI = "TurretAI";

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
                { LaneID.MIDDLE, null },
                { LaneID.BOTTOM, null }}
            },
            {TeamId.TEAM_PURPLE, new Dictionary<LaneID, IInhibitor>{
                { LaneID.TOP, null },
                { LaneID.MIDDLE, null },
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
            {TeamId.TEAM_BLUE, "OrderInhibitor" },
            {TeamId.TEAM_PURPLE, "ChaosInhibitor" }
        };

        //Tower Models
        static Dictionary<TeamId, Dictionary<TurretType, string>> TowerModels { get; set; } = new Dictionary<TeamId, Dictionary<TurretType, string>>
        {
            {TeamId.TEAM_BLUE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "OrderTurretShrine" },
                {TurretType.NEXUS_TURRET, "OrderTurretAngel" },
                {TurretType.INHIBITOR_TURRET, "OrderTurretDragon" },
                {TurretType.INNER_TURRET, "OrderTurretNormal2" },
                {TurretType.OUTER_TURRET, "OrderTurretNormal" },
            } },
            {TeamId.TEAM_PURPLE, new Dictionary<TurretType, string>
            {
                {TurretType.FOUNTAIN_TURRET, "ChaosTurretShrine" },
                {TurretType.NEXUS_TURRET, "ChaosTurretNormal" },
                {TurretType.INHIBITOR_TURRET, "ChaosTurretGiant" },
                {TurretType.INNER_TURRET, "ChaosTurretWorm2" },
                {TurretType.OUTER_TURRET, "ChaosTurretWorm" },
            } }
        };

        //Turret Items
        static Dictionary<TurretType, int[]> TurretItems { get; set; } = new Dictionary<TurretType, int[]>
        {
            { TurretType.OUTER_TURRET, new[] { 1500, 1501, 1502, 1503 } },
            { TurretType.INNER_TURRET, new[] { 1500, 1501, 1502, 1503, 1504 } },
            { TurretType.INHIBITOR_TURRET, new[] { 1501, 1502, 1503, 1505 } },
            { TurretType.NEXUS_TURRET, new[] { 1501, 1502, 1503, 1505 } }
        };

        static IStatsModifier TurretStatsModifier = new StatsModifier();
        static IStatsModifier OuterTurretStatsModifier = new StatsModifier();
        public static void LoadBuildings(Dictionary<GameObjectTypes, List<MapObject>> mapObjects)
        {
            _mapObjects = mapObjects;

            CreateBuildings();
            LoadProtection();

            LoadSpawnBarracks();
            LoadFountains();
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

                foreach (var lane in TurretList[team].Keys)
                {
                    foreach (var turret in TurretList[team][lane])
                    {
                        if (turret.Type == TurretType.FOUNTAIN_TURRET)
                        {
                            continue;
                        }
                        else if (turret.Type != TurretType.NEXUS_TURRET)
                        {
                            TurretHealthModifier.HealthPoints.BaseBonus = 250.0f * Players[enemyTeam].Count;
                        }
                        else
                        {
                            TurretHealthModifier.HealthPoints.BaseBonus = 125.0f * Players[enemyTeam].Count;
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

            //Outer turrets dont get armor
            OuterTurretStatsModifier.MagicResist.FlatBonus = 1;
            OuterTurretStatsModifier.AttackDamage.FlatBonus = 4;
        }

        public static void OnUpdate(float diff)
        {
            var gameTime = GameTime();

            if (gameTime >= timeCheck && timesApplied < 30)
            {
                UpdateTowerStats();
            }

            if (gameTime >= outerTurretTimeCheck && outerTurretTimesApplied < 7)
            {
                UpdateOuterTurretStats();
            }

            foreach (var fountain in FountainList.Values)
            {
                fountain.Update(diff);
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

        public static void OnNexusDeath(IDeathData deathaData)
        {
            var nexus = deathaData.Unit;
            EndGame(nexus.Team, new Vector3(nexus.Position.X, nexus.GetHeight(), nexus.Position.Y), deathData: deathaData);
        }

        public static void OnInhibitorDeath(IDeathData deathData)
        {
            var inhibitor = deathData.Unit as IInhibitor;

            DeadInhibitors[inhibitor.Team].Add(inhibitor, inhibitor.RespawnTime * 1000);

            if (DeadInhibitors[inhibitor.Team].Count == InhibitorList[inhibitor.Team].Count)
            {
                AllInhibitorsAreDead[inhibitor.Team] = true;
            }
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
                        if (turret.Type == TurretType.OUTER_TURRET || turret.Type == TurretType.FOUNTAIN_TURRET || (turret.Type == TurretType.INNER_TURRET && timesApplied >= 20))
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

        static float outerTurretTimeCheck = 30.0f * 1000;
        static byte outerTurretTimesApplied = 0;
        static void UpdateOuterTurretStats()
        {
            foreach (var team in TurretList.Keys)
            {
                foreach (var lane in TurretList[team].Keys)
                {
                    var turret = TurretList[team][lane].Find(x => x.Type == TurretType.OUTER_TURRET);

                    if (turret != null)
                    {
                        turret.AddStatModifier(OuterTurretStatsModifier);
                    }
                }
            }

            outerTurretTimesApplied++;
            outerTurretTimeCheck += 60.0f * 1000;
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
                SpawnBarracks.Add(spawnBarrack.Name, spawnBarrack);
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
                inhibitor.RespawnTime = 240.0f;
                inhibitor.Stats.CurrentHealth = 4000.0f;
                inhibitor.Stats.HealthPoints.BaseValue = 4000.0f;
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

                if (turretType == TurretType.FOUNTAIN_TURRET)
                {
                    continue;
                }

                switch (turretObj.Name)
                {
                    case "Turret_T1_C_06":
                        lane = LaneID.TOP;
                        break;
                    case "Turret_T1_C_07":
                        lane = LaneID.BOTTOM;
                        break;
                }

                var turret = CreateLaneTurret(turretObj.Name + "_A", TowerModels[teamId][turretType], position, teamId, turretType, lane, LaneTurretAI, turretObj);
                TurretList[teamId][lane].Add(turret);
                AddObject(turret);
            }
        }

        static TurretType GetTurretType(int trueIndex, LaneID lane, TeamId teamId)
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
                        if (turret.Type == TurretType.NEXUS_TURRET)
                        {
                            AddProtection(turret, false, TeamInhibitors[inhibitor.Team].ToArray());
                        }
                        else if (turret.Type == TurretType.INHIBITOR_TURRET)
                        {
                            AddProtection(turret, false, TurretList[inhibitor.Team][inhibitor.Lane].First(dependTurret => dependTurret.Type == TurretType.INNER_TURRET));
                        }
                        else if (turret.Type == TurretType.INNER_TURRET)
                        {
                            AddProtection(turret, false, TurretList[inhibitor.Team][inhibitor.Lane].First(dependTurret => dependTurret.Type == TurretType.OUTER_TURRET));
                        }
                    }
                }
            }
        }
    }
}
