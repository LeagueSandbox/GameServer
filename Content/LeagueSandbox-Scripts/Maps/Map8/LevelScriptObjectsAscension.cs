using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;
using System.Collections.Generic;
using GameServerCore.Domain;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace MapScripts.Map8
{
    public static class LevelScriptObjectsAscension
    {
        private static Dictionary<GameObjectTypes, List<MapObject>> _mapObjects;

        static List<IMinion> InfoPoints = new List<IMinion>();
        public static Dictionary<TeamId, IFountain> FountainList = new Dictionary<TeamId, IFountain>();
        static Dictionary<TeamId, List<ILaneTurret>> TurretList = new Dictionary<TeamId, List<ILaneTurret>> { { TeamId.TEAM_BLUE, new List<ILaneTurret>() }, { TeamId.TEAM_PURPLE, new List<ILaneTurret>() } };
        static List<IMinion> TeleportPlates = new List<IMinion>();
        static string LaneTurretAI = "TurretAI";

        public static Dictionary<TeamId, string> TowerModels { get; set; } = new Dictionary<TeamId, string>
        {
            {TeamId.TEAM_BLUE, "OdinOrderTurretShrine" },

            {TeamId.TEAM_PURPLE, "OdinChaosTurretShrine" }
        };

        public static void LoadObjects(Dictionary<GameObjectTypes, List<MapObject>> mapObjects)
        {
            _mapObjects = mapObjects;

            CreateBuildings();
            LoadFountains();
        }

        public static void OnMatchStart()
        {
            LoadShops();
            LoadTeleportPlates();
        }

        public static void OnUpdate(float diff)
        {
            foreach (var fountain in FountainList.Values)
            {
                fountain.Update(diff);
            }
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

        static void CreateBuildings()
        {
            foreach (var turretObj in _mapObjects[GameObjectTypes.ObjAIBase_Turret])
            {
                var teamId = turretObj.GetTeamID();
                var position = new Vector2(turretObj.CentralPoint.X, turretObj.CentralPoint.Z);
                var fountainTurret = CreateLaneTurret(turretObj.Name + "_A", TowerModels[teamId], position, teamId, TurretType.FOUNTAIN_TURRET, LaneID.NONE, LaneTurretAI, turretObj);
                TurretList[teamId].Add(fountainTurret);
                AddObject(fountainTurret);
            }
        }

        static void LoadTeleportPlates()
        {
            foreach (var infoPoint in _mapObjects[GameObjectTypes.InfoPoint])
            {
                var position = new Vector2(infoPoint.CentralPoint.X, infoPoint.CentralPoint.Z);
                NotifySpawnBroadcast(AddPosPerceptionBubble(position, 500.0f, 25000.0f, TeamId.TEAM_BLUE, false));
                NotifySpawnBroadcast(AddPosPerceptionBubble(position, 500.0f, 25000.0f, TeamId.TEAM_PURPLE, false));

                if (infoPoint.Name == "Info_PointA" || infoPoint.Name == "Info_PointB")
                {
                    CreateTeleportPoint(position, TeamId.TEAM_BLUE, "CapturePoint");
                }
                else if (infoPoint.Name == "Info_PointD" || infoPoint.Name == "Info_PointE")
                {
                    CreateTeleportPoint(position, TeamId.TEAM_PURPLE, "CapturePoint");
                }
                else
                {
                    CreateTeleportPoint(position, TeamId.TEAM_BLUE, "NeutralPointOrder");
                    CreateTeleportPoint(position, TeamId.TEAM_PURPLE, "NeutralPointChaos");
                }
            }
        }

        public static void CreateTeleportPoint(Vector2 position, TeamId team, string mapIcon)
        {
            var point = CreateMinion("AscWarpIcon", "AscWarpIcon", position, team: team, ignoreCollision: false, isTargetable: false);
            SetMinimapIcon(point, mapIcon, true);
            TeleportPlates.Add(point);
            point.PauseAi(true);
        }
    }
}
