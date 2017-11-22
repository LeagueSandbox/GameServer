using LeagueSandbox.GameServer.Logic.GameObjects.Statistics;

namespace LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.Buildings.AnimatedBuildings
{
    public class ObjAnimatedBuilding : ObjBuilding
    {
        public ObjAnimatedBuilding(string model, Stats stats, int collisionRadius = 40, 
            float x = 0, float y = 0, int visionRadius = 0, uint netId = 0) : 
            base(model, stats, collisionRadius, x, y, visionRadius, netId)
        {
        }
    }
}
