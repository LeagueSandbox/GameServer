using LeagueSandbox.GameServer.Logic.GameObjects.Stats;

namespace LeagueSandbox.GameServer.Logic.GameObjects.AttackableUnits.Buildings.AnimatedBuildings
{
    public class ObjAnimatedBuilding : ObjBuilding
    {
        public ObjAnimatedBuilding(Game game, string model, Stats.Stats stats, int collisionRadius = 40,
            float x = 0, float y = 0, int visionRadius = 0, uint netId = 0) :
            base(game, model, stats, collisionRadius, x, y, visionRadius, netId)
        {
            Replication = new ReplicationAnimatedBuilding(this);
        }

        public override void Update(float diff)
        {
            base.Update(diff);
            Replication.Update();
        }
    }
}
