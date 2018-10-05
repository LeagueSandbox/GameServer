using GameServerCore.Domain.GameObjects;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings
{
    public class ObjBuilding : AttackableUnit, IObjBuilding
    {
        public ObjBuilding(Game game, string model, IStats stats, int collisionRadius = 40,
            float x = 0, float y = 0, int visionRadius = 0, uint netId = 0) :
            base(game, model, stats, collisionRadius, x, y, visionRadius, netId)
        {
        }
    }
}
