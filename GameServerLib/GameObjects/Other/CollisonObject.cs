using GameServerCore.Domain.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UltimateQuadTree;

namespace LeagueSandbox.GameServer.GameObjects.Other
{
    public class CollisonObject : IQuadTreeObjectBounds<IGameObject>
    {
        public double GetLeft(IGameObject obj)
        {
            return obj.X - obj.CollisionRadius -1;
        }

        public double GetRight(IGameObject obj)
        {
            return obj.X + obj.CollisionRadius +1;
        }

        public double GetTop(IGameObject obj)
        {
            return (20000 - obj.Y) - obj.CollisionRadius -1; //TODO: check if this is the right direction of Y
        }

        public double GetBottom(IGameObject obj)
        {
            return (20000 - obj.Y) + obj.CollisionRadius +1;
        }
    }

}
