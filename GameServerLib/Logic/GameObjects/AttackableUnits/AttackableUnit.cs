using LeagueSandbox.GameServer.Logic.GameObjects.Statistics;

namespace LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits
{
    public class AttackableUnit : Unit
    {
        public AttackableUnit(string model, Stats stats, int collisionRadius = 40,
            float x = 0, float y = 0, int visionRadius = 0, uint netId = 0) : 
            base(model, stats, collisionRadius, x, y, visionRadius, netId)
        {
        }
    }
}
