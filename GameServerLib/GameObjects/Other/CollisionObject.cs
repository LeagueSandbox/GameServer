using GameServerCore.Domain.GameObjects;
using UltimateQuadTree;

namespace LeagueSandbox.GameServer.GameObjects.Other
{
    public class CollisionObject : IQuadTreeObjectBounds<IGameObject>
    {
        public double GetLeft(IGameObject obj)
        {
            return obj.Position.X - obj.CollisionRadius - 1;
        }

        public double GetRight(IGameObject obj)
        {
            return obj.Position.X + obj.CollisionRadius + 1;
        }

        public double GetTop(IGameObject obj)
        {
            return obj.Position.Y - obj.CollisionRadius - 1;
        }

        public double GetBottom(IGameObject obj)
        {
            return obj.Position.Y + obj.CollisionRadius + 1;
        }
    }

}
