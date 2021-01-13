using GameServerCore.Domain.GameObjects;
using System.Numerics;

namespace LeagueSandbox.GameServer.GameObjects.Missiles
{
    public class ObjMissile : GameObject, IObjMissile
    {
        public ObjMissile(Game game, Vector2 position, int collisionRadius, int visionRadius = 0, uint netId = 0) :
            base(game, position, collisionRadius, visionRadius, netId)
        {
        }
    }
}
