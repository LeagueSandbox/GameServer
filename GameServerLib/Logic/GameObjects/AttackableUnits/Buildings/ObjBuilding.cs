namespace LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.Buildings
{
    public class ObjBuilding : AttackableUnit
    {
        public ObjBuilding(string model, Stats.Stats stats, int collisionRadius = 40,
            float x = 0, float y = 0, int visionRadius = 0, uint netId = 0) :
            base(model, stats, collisionRadius, x, y, visionRadius, netId)
        {
        }
    }
}
