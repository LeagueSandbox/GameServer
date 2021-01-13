using GameServerCore.Domain.GameObjects;
using GameServerCore.Enums;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings
{
    public class ObjBuilding : AttackableUnit, IObjBuilding
    {
        public ObjBuilding(Game game, string model, IStats stats, int collisionRadius = 40,
            Vector2 position = new Vector2(), int visionRadius = 0, uint netId = 0, TeamId team = TeamId.TEAM_BLUE) :
            base(game, model, stats, collisionRadius, position, visionRadius, netId, team)
        {
        }
    }
}
