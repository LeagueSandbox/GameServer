using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.Stats;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings
{
    public class ObjAnimatedBuilding : ObjBuilding, IObjAnimatedBuilding
    {
        public ObjAnimatedBuilding(Game game, string model, IStats stats, int collisionRadius = 40,
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
