using GameServerCore.Domain.GameObjects;
using LeagueSandbox.GameServer.GameObjects.Stats;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings.AnimatedBuildings
{
    public class ObjAnimatedBuilding : ObjBuilding, IObjAnimatedBuilding
    {
        public ObjAnimatedBuilding(
            Game game,
            Vector2 position,
            string model,
            Stats.Stats stats,
            int collisionRadius = 40,
            int visionRadius = 0,
            uint netId = 0
        ) : base(game, position, model, stats, collisionRadius, visionRadius, netId)
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
