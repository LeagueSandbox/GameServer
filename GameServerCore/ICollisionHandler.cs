using GameServerCore.Domain.GameObjects;

namespace GameServerCore
{
    /// <summary>
    /// Class which calls to collision based functions for GameObjects.
    /// </summary>
    public interface ICollisionHandler
    {
        void Update();
        void AddObject(IGameObject obj);
        void RemoveObject(IGameObject obj);
    }
}