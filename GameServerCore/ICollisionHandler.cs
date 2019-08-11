using System.Numerics;
using System.Runtime.Versioning;
using GameServerCore.Domain;
using GameServerCore.Domain.GameObjects;

namespace GameServerCore
{
    public interface ICollisionHandler
    {
        void Update();
        void AddObject(IGameObject obj);
        void RemoveObject(IGameObject obj);

        bool IsOccupiedByStaticObject(Vector2 pos, float radius);
        bool IsOccupiedByStaticObject(Vector2 pos);
    }
}