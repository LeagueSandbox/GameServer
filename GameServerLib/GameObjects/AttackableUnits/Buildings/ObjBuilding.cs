using GameServerCore.Enums;
using LeagueSandbox.GameServer.GameObjects.StatsNS;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings
{
    public class ObjBuilding : AttackableUnit
    {
        public override bool IsAffectedByFoW => false;

        public ObjBuilding(Game game, string model, int collisionRadius = 40,
            Vector2 position = new Vector2(), int visionRadius = 0, uint netId = 0, TeamId team = TeamId.TEAM_BLUE, Stats stats = null) :
            base(game, model, collisionRadius, position, visionRadius, netId, team, stats)
        {
        }
    }
}
