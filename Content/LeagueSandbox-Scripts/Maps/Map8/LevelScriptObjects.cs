using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;
using System.Collections.Generic;
using GameServerCore.Domain;
using System.Numerics;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;

namespace MapScripts.Map8
{
    public static class LevelScriptObjects
    {
        private static Dictionary<GameObjectTypes, List<MapObject>> _mapObjects;

        static Dictionary<byte, IMinion> InfoPoints = new Dictionary<byte, IMinion>();
        public static Dictionary<TeamId, IFountain> FountainList = new Dictionary<TeamId, IFountain>();
        static Dictionary<TeamId, List<ILaneTurret>> TurretList = new Dictionary<TeamId, List<ILaneTurret>> { { TeamId.TEAM_BLUE, new List<ILaneTurret>() }, { TeamId.TEAM_PURPLE, new List<ILaneTurret>() } };

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

            foreach (var index in InfoPoints.Keys)
            {
                NotifyHandleCapturePointUpdate(index, InfoPoints[index].NetId, 0, 0, CapturePointUpdateCommand.AttachToObject);
            }
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
                NotifySpawnBroadcast(CreateShop(shop.Name, new Vector2(shop.CentralPoint.X, shop.CentralPoint.Z), shop.GetTeamID()));
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

            byte pointIndex = 0;
            foreach (var infoPoint in _mapObjects[GameObjectTypes.InfoPoint])
            {
                var point = CreateMinion("OdinNeutralGuardian", "OdinNeutralGuardian", new Vector2(infoPoint.CentralPoint.X, infoPoint.CentralPoint.Z), ignoreCollision: true);
                AddUnitPerceptionBubble(point, 800.0f, 25000.0f, TeamId.TEAM_BLUE, true, collisionArea: 120.0f, collisionOwner: point);
                point.PauseAi(true);
                InfoPoints.Add(pointIndex, point);
                pointIndex++;
            }
        }
    }
}
