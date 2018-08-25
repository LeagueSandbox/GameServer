using GameServerCore.Domain.GameObjects;

namespace LeagueSandbox.GameServer.GameObjects.Missiles
{
    public class ObjMissile : GameObject, IObjMissile
    {
        public ObjMissile(Game game, float x, float y, int collisionRadius, int visionRadius = 0, uint netId = 0) :
            base(game, x, y, collisionRadius, visionRadius, netId)
        {
        }
    }
}
