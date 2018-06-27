namespace LeagueSandbox.GameServer.Logic.GameObjects
{
    public class ObjAnimatedBuilding : ObjBuilding
    {
        public ObjAnimatedBuilding(string model, Stats stats, int collisionRadius = 40,
            float x = 0, float y = 0, int visionRadius = 0, uint netId = 0) :
            base(model, stats, collisionRadius, x, y, visionRadius, netId)
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
