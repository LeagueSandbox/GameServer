namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class ObjMissile : GameObject
    {
        public ObjMissile(float x, float y, int collisionRadius, int visionRadius = 0, uint netId = 0) :
            base(x, y, collisionRadius, visionRadius, netId)
        {
        }
    }
}
