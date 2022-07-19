using System.Numerics;
using System.Collections.Generic;
using GameServerCore.Domain;
using GameServerCore.Enums;
using static LeagueSandbox.GameServer.API.ApiMapFunctionManager;
using static LeagueSandbox.GameServer.API.ApiFunctionManager;
using LeagueSandbox.GameServer.GameObjects;
using LeagueSandbox.GameServer.GameObjects.AttackableUnits.AI;

namespace MapScripts.Map8
{
    public static class LevelScriptObjects
    {
        private static Dictionary<GameObjectTypes, List<MapObject>> _mapObjects;

        static List<InfoPoint> InfoPoints = new List<InfoPoint>();
        public static Dictionary<TeamId, Fountain> FountainList = new Dictionary<TeamId, Fountain>();
        static Dictionary<TeamId, List<LaneTurret>> TurretList = new Dictionary<TeamId, List<LaneTurret>> { { TeamId.TEAM_BLUE, new List<LaneTurret>() }, { TeamId.TEAM_PURPLE, new List<LaneTurret>() } };

        //Turret netIds are used for the capture point announcements
        static Dictionary<TeamId, LaneTurret> AnnouncementUnits = new Dictionary<TeamId, LaneTurret>();
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

            foreach (var infoPoint in InfoPoints)
            {
                NotifyHandleCapturePointUpdate(infoPoint.Index, infoPoint.Point.NetId, 0, 0, CapturePointUpdateCommand.AttachToObject);
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
                CreateShop(shop.Name, new Vector2(shop.CentralPoint.X, shop.CentralPoint.Z), shop.GetTeamID());
            }
        }

        static void CreateBuildings()
        {
            foreach (var turretObj in _mapObjects[GameObjectTypes.ObjAIBase_Turret])
            {
                var teamId = turretObj.GetTeamID();
                var position = new Vector2(turretObj.CentralPoint.X, turretObj.CentralPoint.Z);
                var fountainTurret = CreateLaneTurret(turretObj.Name + "_A", TowerModels[teamId], position, teamId, TurretType.FOUNTAIN_TURRET, LaneID.NONE, "TurretAI", turretObj);
                TurretList[teamId].Add(fountainTurret);

                if (!fountainTurret.Name.Contains('1'))
                {
                    AnnouncementUnits.Add(fountainTurret.Team, fountainTurret);
                }

                AddObject(fountainTurret);
            }

            byte pointIndex = 0;
            foreach (var infoPoint in _mapObjects[GameObjectTypes.InfoPoint])
            {
                InfoPoints.Add(new InfoPoint(new Vector2(infoPoint.CentralPoint.X, infoPoint.CentralPoint.Z), pointIndex, infoPoint.Name[infoPoint.Name.Length - 1]));
                pointIndex++;
            }
        }
    }

    class InfoPoint
    {
        public Minion Point;
        public char Id;
        public byte Index;
        public InfoPoint(Vector2 position, byte index, char id)
        {
            Point = CreateMinion("OdinNeutralGuardian", "OdinNeutralGuardian", position, ignoreCollision: true);
            Point.PauseAI(true);
            AddUnitPerceptionBubble(Point, 800.0f, 25000.0f, TeamId.TEAM_BLUE, true, collisionArea: 120.0f);
            AddUnitPerceptionBubble(Point, 800.0f, 25000.0f, TeamId.TEAM_PURPLE, true, collisionArea: 120.0f);

            Id = id;
            Index = index;
        }
    }
}
