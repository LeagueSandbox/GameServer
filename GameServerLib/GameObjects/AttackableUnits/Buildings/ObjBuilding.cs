using GameServerCore.Domain.GameObjects;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.AttackableUnits.Buildings
{
    public class ObjBuilding : AttackableUnit, IObjBuilding
    {
        public ObjBuilding(
            Game game,
            Vector2 position,
            string model,
            Stats.Stats stats,
            int collisionRadius = 40,
            int visionRadius = 0,
            uint netId = 0
        ) : base(game, position, model, stats, collisionRadius, visionRadius, netId)
        {
        }
    }
}
